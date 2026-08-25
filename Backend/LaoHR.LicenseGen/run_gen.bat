@echo off
REM Phase 4D.2 — P1-SEC-001: signing keys are OPERATOR-HELD off-repo.
REM Usage (from Backend\LaoHR.LicenseGen):
REM   set LICENSEGEN_PRIVATE_KEY=C:\path\to\operator\license-signing-private.key
REM   set LICENSEGEN_KEY_PASSPHRASE=...        (only for encrypted PKCS#8 keys)
REM   dotnet run
REM Or pass the key path directly:
REM   dotnet run -- C:\path\to\license-signing-private.key
echo Operator-held key required. See docs/production-phase-4d.2/02-LICENSE-KEY-ROTATION.md
