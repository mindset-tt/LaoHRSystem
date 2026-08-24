# LaoHR / Lao Back Office — production restore script (NO CI/CD)
# Restores a pg_dump custom-format backup into a target database.
# Password is NOT embedded — set PGPASSWORD in the environment.

$ErrorActionPreference = 'Stop'

param(
    [string]$Database = "laohr",
    [string]$Host = "127.0.0.1",
    [int]$Port = 5432,
    [string]$Username = "laohr",
    [string]$Input = "backup.dump"
)

$pgRestore = "C:\Program Files\PostgreSQL\18\bin\pg_restore.exe"

if (-not $env:PGPASSWORD) {
    Write-Error "PGPASSWORD environment variable is not set. Set it before running this script (do NOT embed it here)."
    exit 1
}

if (-not (Test-Path $Input)) {
    Write-Error "Backup file '$Input' not found."
    exit 1
}

Write-Host "=== Restoring '$Input' into '$Database' ==="
& $pgRestore -h $Host -p $Port -U $Username -d $Database --clean --if-exists --no-owner $Input

if ($LASTEXITCODE -ne 0) {
    Write-Error "pg_restore failed."
    exit 1
}

Write-Host "=== Restore complete ==="
