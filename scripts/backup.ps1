# LaoHR / Lao Back Office — PostgreSQL backup script (NO CI/CD)
# Uses pg_dump custom format. Password is NOT embedded — set PGPASSWORD in the
# environment or use a .pgpass file.

$ErrorActionPreference = 'Stop'

param(
    [string]$Database = "laohr_design",
    [string]$Host = "127.0.0.1",
    [int]$Port = 5432,
    [string]$Username = "laohr",
    [string]$Output = "backup.dump"
)

$pgDump = "C:\Program Files\PostgreSQL\18\bin\pg_dump.exe"

if (-not $env:PGPASSWORD) {
    Write-Error "PGPASSWORD environment variable is not set. Set it before running this script (do NOT embed it here)."
    exit 1
}

Write-Host "=== Backing up '$Database' to '$Output' ==="
& $pgDump -h $Host -p $Port -U $Username -Fc -d $Database -f $Output

if ($LASTEXITCODE -ne 0) {
    Write-Error "pg_dump failed."
    exit 1
}

Write-Host "=== Backup complete: $Output ==="
