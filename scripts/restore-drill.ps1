# LaoHR Phase 4D.1 — FULL RESTORE DRILL + RESTORED HTTP SMOKE (NO CI/CD)
#
# Restores a backup produced by scripts/backup-production.ps1 into a CLEAN
# database + CLEAN document storage, boots a SEPARATE API instance against
# them, then performs REAL HTTP smoke checks through that instance.
#
# Measured and reported: restore duration, application recovery duration.
#
# Requires: POSTGRES_PASSWORD env var.
# Usage:
#   .\scripts\restore-drill.ps1 -BackupDir backup\20260825-021310 `
#       -OffhostCopy C:\path\offhost\<stamp>   # optional but recommended

param(
    [Parameter(Mandatory = $true)][string]$BackupDir,
    [string]$OffhostCopy = "",
    [string]$RestoreDbName = "laohr_restore",
    [string]$PgContainer = "laohr-prodlike-postgres",
    [string]$ApiImage = "prodlike4d1-api",
    [int]$SmokePort = 18080,
    [switch]$SkipOffhostUse
)

$ErrorActionPreference = 'Stop'
# Bind-mounts require absolute paths.
$BackupDir = [System.IO.Path]::GetFullPath(($ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($BackupDir)))
if ($OffhostCopy) { $OffhostCopy = [System.IO.Path]::GetFullPath(($ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OffhostCopy))) }

function Fail([string]$msg) { Write-Error "RESTORE DRILL FAILED: $msg"; exit 1 }
function Step([string]$m) { Write-Host "`n=== $m ===" }
function Elapsed([datetime]$from) { return [math]::Round(((Get-Date) - $from).TotalSeconds, 1) }

if (-not $env:POSTGRES_PASSWORD) { Fail "POSTGRES_PASSWORD not set." }

# Prefer verifying the off-host copy if given (that's what you'd have in a real DR).
$sourceDir = $BackupDir
if ($OffhostCopy -and -not $SkipOffhostUse) {
    Step "Verifying off-host copy integrity before restore"
    $localHashes = Get-Content (Join-Path $BackupDir 'checksums.sha256')
    foreach ($h in $localHashes) {
        $parts = $h -split '\s{2}'; $expected = $parts[0]; $fname = $parts[1]
        $copyPath = Join-Path $OffhostCopy $fname
        if (-not (Test-Path $copyPath)) { Fail "off-host copy missing file $fname" }
        $actual = (Get-FileHash $copyPath -Algorithm SHA256).Hash
        if ($actual -ne $expected) { Fail "off-host copy hash mismatch: $fname" }
    }
    Write-Host "    off-host copy matches local checksums."
}

Step "Preconditions"
$dbDump = Join-Path $sourceDir 'laohr-db.dump'
$docArchive = Join-Path $sourceDir 'documents.tgz'
foreach ($f in @($dbDump, $docArchive)) { if (-not (Test-Path $f)) { Fail "missing $f" } }

Step "Dropping any previous restore target"
docker exec $PgContainer psql -U laohr -d postgres -c "DROP DATABASE IF EXISTS ""$RestoreDbName"";"
if ($LASTEXITCODE -ne 0) { Fail "could not drop previous restore DB." }

Step "Restoring PostgreSQL into clean database '$RestoreDbName'"
$t0 = Get-Date
docker exec $PgContainer createdb -U laohr $RestoreDbName
docker cp $dbDump "${PgContainer}:/tmp/drill-dump.bin"
docker exec -e PGPASSWORD="$env:POSTGRES_PASSWORD" $PgContainer `
    pg_restore -U laohr -d $RestoreDbName --no-owner --role=laohr /tmp/drill-dump.bin
$rc = $LASTEXITCODE
docker exec $PgContainer rm /tmp/drill-dump.bin
if ($rc -ne 0) { Fail "pg_restore exited $rc." }
$restoreSecs = Elapsed $t0
Write-Host ("    DB restore took {0}s" -f $restoreSecs)

Step "Restoring document storage into fresh volume"
$docVolume = "laohr-drill-documents-$stamp-" + (Get-Random)
docker volume create $docVolume | Out-Null
$t1 = Get-Date
docker run --rm -v "${docVolume}:/docs" -v "${sourceDir}:/backup:ro" alpine `
    sh -c "tar xzf /backup/documents.tgz -C /docs"
if ($LASTEXITCODE -ne 0) { Fail "document extraction failed." }
$docsSecs = Elapsed $t1
Write-Host ("    document restore took {0}s" -f $docsSecs)

Step "Booting separate API against restored environment"
$jwt = -join ((1..64) | ForEach-Object { [char](Get-Random -InputObject (48..57 + 65..90 + 97..122)) })
$container = "laohr-drill-api"
docker rm -f $container 2>$null | Out-Null
$t2 = Get-Date
docker run -d --name $container `
    --network prodlike4d1_prodlike `
    -p "127.0.0.1:${SmokePort}:8080" `
    -e ASPNETCORE_ENVIRONMENT=Production `
    -e ASPNETCORE_URLS=http://+:8080 `
    -e "ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=$RestoreDbName;Username=laohr;Password=$env:POSTGRES_PASSWORD" `
    -e "Jwt__Key=$jwt" `
    -e Jwt__Issuer=LaoHRServer -e Jwt__Audience=LaoHRClient `
    -e "Cors__AllowedOrigins=https://localhost" `
    -e NPGSQL_LEGACY_TIMESTAMP=1 `
    -e Storage__DocumentsRoot=/data/documents `
    -v "${docVolume}:/data/documents" `
    $ApiImage | Out-Null
if ($LASTEXITCODE -ne 0) { Fail "drill API container failed to start." }

Write-Host "    waiting for readiness..."
$readyUpTo = 120
do {
    Start-Sleep -Seconds 3
    $code = & curl.exe -s -o NUL -w "%{http_code}" "http://127.0.0.1:$SmokePort/health/ready"
    $waited = Elapsed $t2
} while ($code -ne '200' -and $waited -lt $readyUpTo)

if ($code -ne '200') { Fail "restored API did not become healthy within ${readyUpTo}s." }
$appRecoverySecs = Elapsed $t2
Write-Host ("    application recovery (container start -> ready): {0}s" -f $appRecoverySecs)

Step "RESTORED HTTP SMOKE (real HTTP calls)"
$base = "http://127.0.0.1:$SmokePort"
$failures = 0

$live = & curl.exe -s -o NUL -w "%{http_code}" "$base/health/live"
$ready = & curl.exe -s -o NUL -w "%{http_code}" "$base/health/ready"
Write-Host "    GET /health/live  -> $live"; if ($live -ne '200') { $failures++ }
Write-Host "    GET /health/ready -> $ready"; if ($ready -ne '200') { $failures++ }

$loginBody = '{"username":"admin","password":"LoadTest!2026"}'
$login = & curl.exe -s -X POST "$base/api/auth/login" -H "Content-Type: application/json" -d $loginBody
try { $token = ($login | ConvertFrom-Json).token } catch { $token = $null }
if ($token) { Write-Host "    POST /api/auth/login -> OK (token issued)" } else { Write-Host "    POST /api/auth/login -> FAILED"; $failures++ }

$authedChecks = @(
    @{ name = 'GET /api/employees';          url = '/api/employees?page=1&pageSize=5' },
    @{ name = 'GET /api/suppliers';          url = '/api/suppliers?page=1&pageSize=5' },
    @{ name = 'GET /api/supplier-invoices/aging (finance)'; url = '/api/supplier-invoices/aging' },
    @{ name = 'GET /api/corporate-documents (corporate)';   url = '/api/corporate-documents?page=1&pageSize=5' },
    @{ name = 'GET /api/journals (GL)';      url = '/api/journals?page=1&pageSize=5' }
)
foreach ($c in $authedChecks) {
    $sc = & curl.exe -s -o NUL -w "%{http_code}" -H "Authorization: Bearer $token" "$base$($c.url)"
    Write-Host ("    {0} -> {1}" -f $c.name, $sc)
    if ($sc -ne '200') { $failures++ }
}

Step "Document restore verification through API (checksum)"
# Find one uploaded test document row in restored DB and fetch it.
$docRow = docker exec $PgContainer psql -U laohr -d $RestoreDbName -t -A -F'|' -c `
    "SELECT d.""DocumentId"", d.""FileName"" FROM ""EmployeeDocuments"" d WHERE d.""FilePath"" LIKE 'documents/%' ORDER BY d.""DocumentId"" DESC LIMIT 1;"
if (-not $docRow) { Fail "no documents found in restored DB." }
$docId, $docName = ($docRow -split '\|')
$tmpFile = Join-Path $env:TEMP "drill-doc-$docId"
$dl = & curl.exe -s -o $tmpFile -w "%{http_code}" -H "Authorization: Bearer $token" "$base/api/documents/$docId/file"
Write-Host "    GET /api/documents/$docId/file -> $dl"
if ($dl -ne '200') { Fail "document download failed." }

# Compare against the bytes inside the ORIGINAL archive.
$origExtract = Join-Path $env:TEMP "drill-doc-verify"
New-Item -ItemType Directory -Force -Path $origExtract | Out-Null
$key = docker exec $PgContainer psql -U laohr -d $RestoreDbName -t -A -c `
    "SELECT ""FilePath"" FROM ""EmployeeDocuments"" WHERE ""DocumentId"" = $docId;"
docker run --rm -v "${docVolume}:/docs:ro" -v "${origExtract}:/out" alpine sh -c "cp `"/docs/$key`" /out/orig.bin"
$hashA = (Get-FileHash $tmpFile -Algorithm SHA256).Hash
$hashB = (Get-FileHash (Join-Path $origExtract 'orig.bin') -Algorithm SHA256).Hash
if ($hashA -eq $hashB) { Write-Host "    CHECKSUM MATCH: $hashA" } else { Fail "checksum mismatch: served=$hashA storage=$hashB" }
Remove-Item $tmpFile, (Join-Path $origExtract 'orig.bin') -Force -ErrorAction SilentlyContinue

Step "Summary"
Write-Host (@"
DB restore duration:          ${restoreSecs}s
Document restore duration:    ${docsSecs}s
App recovery (to ready):      ${appRecoverySecs}s
HTTP smoke failures:          $failures (must be 0)
Restored doc checksum:        $(if ($hashA -eq $hashB) {'MATCH'} else {'MISMATCH'})
"@)

if ($failures -gt 0 -or $hashA -ne $hashB) { exit 1 }

# Cleanup drill containers/volumes (keep volumes for inspection until teardown).
docker rm -f $container | Out-Null
docker volume rm $docVolume 2>$null | Out-Null
Write-Host "`n=== RESTORE DRILL PASSED ==="
exit 0
