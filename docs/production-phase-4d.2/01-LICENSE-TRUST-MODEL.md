# 01 — LICENSE TRUST MODEL

Verified by reading the code (not assumed) this phase.

## Components

| Component | File | Role |
|---|---|---|
| LicenseGen | `Backend/LaoHR.LicenseGen/Program.cs` | Offline operator tool; **signs** licenses with the PRIVATE key |
| LicenseData | `Backend/LaoHR.Shared/Models/LicenseData.cs` | Payload model (LicenseId, CustomerName, Type, IssuedAt, ExpirationDate, MaxEmployees, HardwareId, Features) |
| LicenseService | `Backend/LaoHR.Shared/Services/LicenseService.cs` | **Verifies** license signature against `public.key`; rejects expired (`ExpirationDate < UtcNow`) |
| LicenseMiddleware | `Backend/LaoHR.API/Middleware/LicenseMiddleware.cs` | Loads license from DB `SystemSettings.LICENSE_KEY`, enforces 402 on invalid; bypasses /swagger, /api/auth, /api/license, /health, /metrics |
| LicenseKeyCache | `Backend/LaoHR.API/Services/LicenseKeyCache.cs` | 5-min positive TTL / 15-s negative TTL cache of verification result |

## Format and cryptography

```
license := base64url-safe?( payloadJSON ) "." base64( RSA-SHA256-PKCS1( payloadBytes ) )
payloadJSON := Newtonsoft JSON serialization of LicenseData
verification anchor := PEM SubjectPublicKeyInfo at application base directory
                       (LaoHR.API/public.key, copied to output via csproj)
```

- Signature: RSA 2048, SHA-256, PKCS#1 v1.5 padding (`SignData`/`VerifyData`).
- The private key is the **signing root**: whoever holds it can mint licenses
  for any customer/expiry/limits. It must never be readable by the app,
  repo, images, or backups.
- The public key ships with the application and may be freely committed.

## Trust flow

1. Operator signs a license off-repo → delivers license string to customer DB
   (`SystemSettings.LICENSE_KEY`).
2. API middleware reads it (cached), verifies RSA signature against embedded
   public key, checks expiry.
3. Invalid/expired/forged ⇒ HTTP 402 for all licensed endpoints.

## Consequence of compromise (pre-4D.2 state)

Because the private key was in git history since the initial commit AND pushed
to GitHub, any reader could mint unlimited valid ENTERPRISE licenses.
Therefore rotation was mandatory — see 02.
