# 02 — LICENSE KEY ROTATION (P1-SEC-001)

## Classification of the old key

`OLD_LICENSE_SIGNING_KEY = COMPROMISED / RETIRED`

Evidence: `Backend/LaoHR.LicenseGen/private.key` existed from the initial
commit (`9dfea53`) and is present in `origin/dev-v4`, `origin/master`,
`origin/dev` on GitHub (public remote). Deleting the working-tree file alone
would NOT remediate this — hence history purge (below) + rotation.

Old keypair fingerprint (SHA256, for audit identification only):
- private.key: `A9599D391234B4BD6FE99831B62D3BB8DBD52B6DFA8329DC8DF8890A4E830DF0`
- matching public keys (LicenseGen copy == API copy):
  `6A0B46C3A1B928CACCAC736A24F6F8DE86B3033EBCA5F61ACAF42609F7A07D34`

## New key generation (operator process)

Generated OUTSIDE the repository with OpenSSL-backed Node crypto:

| Artifact | Location | Form |
|---|---|---|
| NEW signing key | `C:\Users\rl34996\laohr-operator-keys\license-signing-private.key` | **ENCRYPTED PKCS#8 PEM** (AES-256-CBC), 48-char random passphrase |
| passphrase | same directory, `passphrase.txt` | machine-local stand-in until operator migrates to secure storage |
| NEW verification key | same dir + committed at `Backend/LaoHR.API/public.key` | SPKI PEM |

Notes:
- Generation ran on the operator workstation; nothing was printed to console,
  logs, chat, or docs. File ACLs are user-profile default (single-user).
- The operator MUST move the encrypted key + passphrase to the real secure
  store (offline media / org password vault) and delete machine-local copies
  when the deployment host is finalized. This is recorded as an operator TODO
  in 12-NEXT-PHASE-HANDOFF.
- The retired key was preserved off-repo ONLY to sign a rejection-evidence
  license; it must be destroyed by the operator after acceptance.

## Repository changes

1. `git rm --cached Backend/LaoHR.LicenseGen/private.key public.key license.key`
   → active tree no longer tracks any LicenseGen key material.
2. `.gitignore`: explicit ignore rules added for those three paths (+ `*.pem.key`).
3. `Backend/LaoHR.API/public.key` replaced with the NEW public key:
   SHA256 `999FB49AB83314DDCEFA95FD0A66FBED2C8A898340747B2869500E94EA3D10C1`.
4. `LaoHR.LicenseGen/Program.cs` rewritten:
   - NEVER generates or writes keys into its CWD anymore.
   - Requires operator-held key path via `arg[0]` or `LICENSEGEN_PRIVATE_KEY`.
   - Detects `ENCRYPTED PRIVATE KEY` PEMs and uses `ImportFromEncryptedPem`
     with `LICENSEGEN_KEY_PASSPHRASE`.
   - Missing key ⇒ loud error + exit 1 (no fallback generation).

## Git history purge (destructive — APPROVED)

Executed after all phase commits (see final commit list):

```
pip install git-filter-repo
git filter-repo --force --invert-paths `
  --path Backend/LaoHR.LicenseGen/private.key `
  --path Backend/LaoHR.LicenseGen/license.key `
  --path Backend/LaoHR.LicenseGen/public.key
```

- Rewrites every ref that contains those paths (all branches share the initial
  commit ancestry). EF migration files and their order are untouched.
- filter-repo strips the `origin` remote by design; it was re-added and all
  rewritten branches force-pushed (see 04 for post-verification).
- GitHub-side residual caches (PR refs, forks, cached views) cannot be purged
  by push alone. Operator follow-up recorded in 12: request GitHub Support
  secret-removal / gc if the repo remains public, else make repo private.

## Rotation live test (real stack, real HTTP)

Prodlike topology booted fresh on final code; license swapped in DB per case;
API container restarted between cases to clear the 5-min positive cache.

| Case | Result |
|---|---|
| NEW legit license | **HTTP 200** (login + authorized `/api/employees`) |
| OLD retired-key license | **HTTP 402 REJECTED** |
| TAMPERED payload (MaxEmployees→999999) | **HTTP 402 REJECTED** |
| EXPIRED new-key license | **HTTP 402 REJECTED** |
| GARBAGE string | **HTTP 402 REJECTED** |
| Restored NEW license | **HTTP 200** |

Unit-level rotation regression: `Backend/LaoHR.Tests/Unit/Services/LicenseRotationTests.cs`
(4 tests; old-key-signed license fails under new anchor, new-key passes,
expired/tampered fail). Full suite green — see 10-FINAL-TESTS.md.

## Final classification

```
OLD_PRIVATE_KEY:              RETIRED / REMOVED (history purge verified in 04)
NEW_PRIVATE_KEY_IN_GIT:       NO
NEW_PRIVATE_KEY_IN_IMAGE:     NO
NEW_PRIVATE_KEY_IN_BACKUP:    NO
LICENSE_REISSUANCE_REQUIRED:  YES (strategy A executed — see 03)
```
