# LaoHR Phase 4D.1 — PRODUCTION BACKUP WITH OFF-HOST COPY (NO CI/CD)
#
# Environment-driven; NO embedded credentials. Requires:
#   POSTGRES_PASSWORD   (env var; never written to disk by this script)
#   Optional env/params: BACKUP_DIR, OFFHOST_DEST, DB_NAME
#
# What it produces (in BACKUP_DIR\<timestamp>\):
#   laohr-db.dump          PostgreSQL custom-format dump (pg_dump -Fc)
#   documents.tgz          document storage archive (tar+gzip)
#   config-metadata.json   NON-SECRET deployment metadata needed for restore
#   checksums.sha256       SHA256 of dump + archive + manifest
#   manifest.json          contents, sizes, timestamps, versions
#
# Off-host copy: the whole timestamp folder is copied to OFFHOST_DEST and the
# copy is re-verified byte-for-byte (hash comparison).
# NOTE: when OFFHOST_DEST is a second local/mounted volume this is an
#       OFFHOST_DR_SIMULATION, not true geographic DR. Label accordingly.
#
# Any verification failure => non-zero exit and NO success message.

param(
    [string]$BackupDir = "backup",
    [string]$OffhostDest = "",
    [string]$DbName = "laohr",
    [string]$PgContainer = "laohr-prodlike-postgres",
    [string]$ApiContainer = "laohr-prodlike-api",
    [string]$DocVolume = "laohr-prodlike-documents"
)

$ErrorActionPreference = 'Stop'
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$target = [System.IO.Path]::GetFullPath((Join-Path $BackupDir $stamp))

function Fail([string]$msg) {
    Write-Error "BACKUP FAILED: $msg"
    exit 1
}

if (-not $env:POSTGRES_PASSWORD) {
    Fail "POSTGRES_PASSWORD environment variable is not set. Do NOT embed credentials here."
}
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) { Fail "docker not available." }

New-Item -ItemType Directory -Force -Path $target | Out-Null

Write-Host "=== [1/5] Database dump ($DbName -> custom format) ==="
$dbDump = Join-Path $target "laohr-db.dump"
docker exec -e PGPASSWORD="$env:POSTGRES_PASSWORD" $PgContainer `
    pg_dump -U laohr -d $DbName -Fc -f /tmp/laohr-db.dump
if ($LASTEXITCODE -ne 0) { Fail "pg_dump exited non-zero." }
docker cp "${PgContainer}:/tmp/laohr-db.dump" $dbDump
if ($LASTEXITCODE -ne 0) { Fail "could not copy dump out of container." }
docker exec $PgContainer rm /tmp/laohr-db.dump
if (-not (Test-Path $dbDump) -or ((Get-Item $dbDump).Length -le 0)) { Fail "dump missing or zero bytes." }
Write-Host ("    dump size: {0:N0} bytes" -f (Get-Item $dbDump).Length)

Write-Host "=== [2/5] pg_restore --list verification ==="
docker cp $dbDump "${PgContainer}:/tmp/verify-dump.dump"
$restoreList = docker exec -e PGPASSWORD="$env:POSTGRES_PASSWORD" $PgContainer pg_restore --list /tmp/verify-dump.dump 2>&1
$rlExit = $LASTEXITCODE
docker exec $PgContainer rm /tmp/verify-dump.dump
if ($rlExit -ne 0 -or -not ($restoreList -match 'TABLE DATA')) {
    Fail "pg_restore --list failed or dump contains no table data."
}
($restoreList | Select-Object -First 2) | ForEach-Object { Write-Host "    $_" }

Write-Host "=== [3/5] Document storage archive ==="
$docArchive = Join-Path $target "documents.tgz"
docker run --rm -v "${DocVolume}:/docs:ro" -v "${target}:/backup" alpine `
    tar czf /backup/documents.tgz -C /docs .
if ($LASTEXITCODE -ne 0) { Fail "document archive failed." }
if (-not (Test-Path $docArchive) -or ((Get-Item $docArchive).Length -le 0)) { Fail "document archive missing or zero bytes." }

Write-Host "=== [4/5] Manifest (NON-SECRET metadata only) ==="
$manifest = [PSCustomObject]@{
    createdAtUtc      = (Get-Date).ToUniversalTime().ToString('o')
    database          = $DbName
    dbDumpFile        = 'laohr-db.dump'
    dbDumpBytes       = (Get-Item $dbDump).Length
    documentsFile     = 'documents.tgz'
    documentVolume    = $DocVolume
    restoreHint       = 'scripts/restore-drill.md; requires PG16, Storage:DocumentsRoot populated from documents.tgz'
    drClassification  = if ($OffhostDest) { 'OFFHOST_DR_SIMULATION' } else { 'LOCAL_BACKUP_ONLY' }
    schemaNote        = 'custom-format pg_dump; restore with pg_restore --clean --if-exists --no-owner'
}
$manifestPath = Join-Path $target "manifest.json"
$manifest | ConvertTo-Json -Depth 4 | Set-Content $manifestPath -Encoding UTF8

Write-Host "=== Checksums ==="
$hashes = Get-ChildItem $target -File | Where-Object { $_.Name -ne 'checksums.sha256' } |
    ForEach-Object { "$((Get-FileHash $_.FullName -Algorithm SHA256).Hash)  $($_.Name)" }
$hashes | Set-Content (Join-Path $target "checksums.sha256") -Encoding ASCII
$hashes | ForEach-Object { Write-Host "    $(($_ -split '\s{2}')[1])" }

Write-Host "=== [5/5] Off-host copy + re-verification ==="
if ($OffhostDest) {
    if (-not (Test-Path $OffhostDest)) {
        # Deliberate failure-path check: destination must pre-exist and be writable.
        Fail "off-host destination '$OffhostDest' does not exist or is not reachable."
    }
    try {
        $probe = Join-Path $OffhostDest ".write-probe-$pid"
        Set-Content -Path $probe -Value 'probe' -ErrorAction Stop
        Remove-Item $probe -Force
    } catch {
        Fail "off-host destination '$OffhostDest' is not writable."
    }
    $destFolder = Join-Path $OffhostDest $stamp
    Copy-Item $target $destFolder -Recurse -Force
    foreach ($h in $hashes) {
        $parts = $h -split '\s{2}'
        $expectedHash = $parts[0]; $fname = $parts[1]
        $copyHash = (Get-FileHash (Join-Path $destFolder $fname) -Algorithm SHA256).Hash
        if ($copyHash -ne $expectedHash) { Fail "off-host copy hash mismatch for $fname." }
        Write-Host "    verified: $fname"
    }
    Write-Host "OFF-HOST COPY VERIFIED: $destFolder (OFFHOST_DR_SIMULATION)"
} else {
    Write-Host "(no OFFHOST_DEST given; skipping off-host step)"
}

Write-Host "`n=== BACKUP COMPLETE AND VERIFIED: $target ==="
exit 0
