# 03 — LICENSE RE-ISSUANCE

Decision (operator-approved): **Strategy A — reissue all licenses; no dual-key window.**

## Why A is sufficient

- System is pre-commercial: the only license ever issued was the demo
  ENTERPRISE license (`license.key`, signed by the retired key, itself only in
  git history / local DBs).
- Dual-key verification would add a second trust anchor and more code paths —
  new attack surface with zero current benefit. Rejected by design.

## Consequence of rotation

Any license signed by the retired key fails verification under the new
`public.key` (proven live: HTTP 402). Every legitimate deployment must receive
a newly-signed license before upgrading an API instance that carries the new
public key.

## Reissue procedure (per customer)

1. Operator machine with secure access to `license-signing-private.key`
   (+ passphrase if encrypted PKCS#8).
2. Generate:
   ```
   cd Backend\LaoHR.LicenseGen
   set LICENSEGEN_PRIVATE_KEY=<secure-path>\license-signing-private.key
   set LICENSEGEN_KEY_PASSPHRASE=<passphrase>
   dotnet run -- <keypath>        # or env var only
   ```
   → prints license string, writes untracked `license.key`.
   Edit customer/expiry/limits in `GenerateLicense()` as required per delivery.
3. Deliver ONLY the license string to the customer (e-mail/vault — it is
   signature-protected data, not a secret, but treat as contract data).
4. Customer applies:
   ```sql
   UPDATE "SystemSettings" SET "SettingValue" = '<license>' WHERE "SettingKey" = 'LICENSE_KEY';
   ```
5. Verification: any licensed endpoint returns 200 within 15 s–5 min
   (negative cache 15 s / positive 5 min); restart clears immediately.

## This phase's reissuance evidence

New demo ENTERPRISE license (365 d, MaxEmployees=100) signed with the NEW key,
installed into the prodlike DB, verified end-to-end via real HTTP login +
authorized fetch (HTTP 200). Old-key license rejected (402).

LICENSE_REISSUANCE_REQUIRED = YES — executed for all existing (demo) holders.
