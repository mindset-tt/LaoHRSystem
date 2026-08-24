# 09 — ADR Register

> Phase 3A architectural decisions.

## ADR-01: Password Hashing — PBKDF2 over Argon2id

**Date**: 2026-08-21  
**Status**: Accepted

### Context
Existing system uses unsalted SHA-256 (single-pass) for password hashing. OWASP recommends Argon2id as #1 choice, PBKDF2 as acceptable for FIPS environments. Phase 2B research recommended Argon2id via `Konscious.Security.Cryptography.Argon2`.

### Decision
Use **PBKDF2-HMAC-SHA256** with 600,000 iterations (OWASP recommendation), 128-bit salt, 256-bit hash. Format: `v2:{base64salt}:{base64hash}`.

### Rationale
- **No external dependency** — `Rfc2898DeriveBytes.Pbkdf2` is built into .NET 10.
- **OWASP acceptable** — PBKDF2 with HMAC-SHA256 at 600k iterations is OWASP-recommended for FIPS environments.
- **Argon2id** requires third-party library (`Konscious.Security.Cryptography.Argon2`) adding a dependency and maintenance burden.
- **Migration strategy** is identical (rehash-on-login with version flag).
- **Constant-time comparison** via `CryptographicOperations.FixedTimeEquals`.

### Consequences
- New users + rehashed users get `PasswordHashVersion = 2` (PBKDF2).
- Legacy users (version 1, SHA-256) are migrated on next successful login.
- `AppUser.PasswordHashVersion` column added (default 1 = legacy).
- `PasswordHasher` supports both versions with `VerifyPasswordWithMigration()`.

## ADR-02: Global Exception Handling — IExceptionHandler + ProblemDetails

**Date**: 2026-08-21  
**Status**: Accepted

### Context
No global exception handler existed. Unhandled exceptions could leak stack traces in Development.

### Decision
Use built-in .NET 8+ `IExceptionHandler` + `AddProblemDetails()` producing RFC 7807 ProblemDetails JSON responses.

### Rationale
- Built into .NET 10 (VERIFIED — Microsoft Learn, aspnetcore-10.0).
- No external dependency.
- RFC 7807 standardized error format.
- Stack traces hidden in non-Development environments.

## ADR-03: JWT Fail-Fast in Production

**Date**: 2026-08-21  
**Status**: Accepted

### Context
Program.cs had a hardcoded JWT fallback key used when `Jwt:Key` config was missing — a security risk in Production.

### Decision
In Production/Staging, throw `InvalidOperationException` if `Jwt:Key` is missing. In Development/Testing, use the hardcoded fallback.

### Rationale
- Prevents silent use of a publicly-known signing key in production.
- Maintains Development ergonomics (no env var needed for local dev).

## ADR-04: decimal.Parse InvariantCulture for Payroll Settings

**Date**: 2026-08-21  
**Status**: Accepted

### Context
`PayrollService.LoadSettingsAsync()` used `decimal.Parse()` without `CultureInfo.InvariantCulture`, causing locale-dependent parsing of NSSF rates (e.g., "0.055" parsed as 55 instead of 0.055 on some locales).

### Decision
Use `decimal.Parse(value, CultureInfo.InvariantCulture)` for all settings parsing in `PayrollService`.

### Rationale
- **Critical bug fix** — without InvariantCulture, NSSF deductions would be 1000× too high in production.
- This was a pre-existing bug discovered during Phase 3A testing.

## ADR-05: PIT Bracket Update — Law No. 88/NA

**Date**: 2026-08-21  
**Status**: Accepted

### Context
PIT tax-free threshold changed from LAK 1,300,000 to LAK 2,500,000 per Amended Income Tax Law No. 88/NA (25 June 2025, effective July 2026). VERIFIED by PwC Worldwide Tax Summaries.

### Decision
Update `TaxBracket` seed in `LaoHRDbContext.OnModelCreating` to the verified brackets: 0/5/10/15/20/25% at 2.5M/5M/15M/25M/65M.

### Rationale
- PwC Worldwide Tax Summaries (quality 5/5) confirms brackets.
- Codebase brackets 3-6 were already correct; brackets 1-2 were outdated (1.3M threshold).
- Rule IDs: LAO-PIT-2026-BRACKET-01 through 06 (LOCKED).