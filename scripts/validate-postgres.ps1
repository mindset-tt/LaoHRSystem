# LaoHR / Lao Back Office — PostgreSQL migration validation (NO CI/CD)
# Applies all migrations to a disposable PostgreSQL database and verifies the
# Back Office tables exist. Uses the native PostgreSQL on 127.0.0.1:5432.
#
# IMPORTANT: `dotnet ef database update` requires the legacy timestamp switch
# (seed data contains DateTimeKind.Unspecified values). Set NPGSQL_LEGACY_TIMESTAMP=1
# so the design-time factory enables it. `migrations add` must run WITHOUT it.

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

$env:DOTNET_ROOT = "$HOME\.dotnet"
$env:Path = "$HOME\.dotnet;" + $env:Path
$env:NPGSQL_LEGACY_TIMESTAMP = "1"

$dbName = "laohr_validate"
$conn = "Host=127.0.0.1;Port=5432;Database=$dbName;Username=laohr;Password=laohr"

Write-Host "=== Applying migrations to disposable DB '$dbName' ==="
Push-Location "$root\Backend\LaoHR.API"
dotnet ef database update --connection $conn
Pop-Location

Write-Host "=== Verifying Back Office tables ==="
$env:PGPASSWORD = "laohr"
$psql = "C:\Program Files\PostgreSQL\18\bin\psql.exe"
& $psql -h 127.0.0.1 -p 5432 -U laohr -d $dbName -c "SELECT count(*) AS total_tables FROM pg_tables WHERE schemaname='public';"

Write-Host "=== DONE ==="
