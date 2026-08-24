# LaoHR / Lao Back Office — Local validation script (NO CI/CD)
# Runs backend restore/build/tests and frontend typecheck/tests/build/lint.
# No cloud runners, no GitHub Actions, no paid CI minutes.

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

# .NET 10 SDK is installed locally (not on PATH by default).
$env:DOTNET_ROOT = "$HOME\.dotnet"
$env:Path = "$HOME\.dotnet;" + $env:Path

# Test-only JWT key (never used in production).
$env:ASPNETCORE_ENVIRONMENT = "Testing"
$env:Jwt__Key = "ci-only-test-key-not-for-production-do-not-use-64+chars-long-string"
$env:Jwt__Issuer = "LaoHRServer"
$env:Jwt__Audience = "LaoHRClient"

Write-Host "=== BACKEND: restore ==="
Push-Location "$root\Backend"
dotnet restore
Write-Host "=== BACKEND: build ==="
dotnet build --no-restore
Write-Host "=== BACKEND: test ==="
Push-Location "$root\Backend\LaoHR.Tests"
dotnet test --no-restore
Pop-Location
Pop-Location

Write-Host "=== FRONTEND: typecheck ==="
Push-Location "$root\frontend"
npx tsc --noEmit
Write-Host "=== FRONTEND: test ==="
npm test
Write-Host "=== FRONTEND: build ==="
npm run build
Write-Host "=== FRONTEND: lint (summary) ==="
npm run lint
Pop-Location

Write-Host "=== DONE ==="
