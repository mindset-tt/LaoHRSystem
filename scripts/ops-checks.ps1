# LaoHR Phase 4D.1 — OPERATIONAL HEALTH CHECKS (manual / cron-able)
#
# INTERNAL_TECHNICAL_DEFAULTS — thresholds are engineering configuration
# defaults, NOT contractual SLAs. Adjust per deployment capacity.
#
# Checks (each exits non-zero on failure):
#   1. health/ready failing          -> CRITICAL (app cannot serve)
#   2. PostgreSQL unreachable        -> CRITICAL
#   3. disk usage > threshold        -> WARNING/CRITICAL
#   4. document storage unavailable  -> CRITICAL (uploads/downloads fail)
#   5. excessive 5xx rate            -> CRITICAL
#   6. high process memory           -> WARNING
#   7. DB connection saturation      -> WARNING
#   8. backup freshness              -> WARNING (no backup within window)
#
# Usage: .\scripts\ops-checks.ps1 [-BaseUrl https://localhost] [-PgPassword ...]
# Exit code = highest severity hit (0 ok, 1 warning, 2 critical).

param(
    [string]$BaseUrl = "https://localhost",
    [int]$DiskWarnPct = 75,
    [int]$DiskCritPct = 90,
    [long]$ApiMemWarnMb = 1024,
    [int]$PgConnSaturationPct = 85,     # % of max_connections
    [int]$BackupMaxAgeHours = 26,
    [string]$BackupDir = "backup",
    [switch]$InsecureTls                # self-signed test topology
)

$ErrorActionPreference = 'Continue'
$script:worst = 0   # 0 ok / 1 warn / 2 crit

function Set-Severity([int]$level) { if ($level -gt $script:worst) { $script:worst = $level } }
function Warn([string]$msg)  { Write-Host "[WARN ] $msg"; Set-Severity 1 }
function Crit([string]$msg)  { Write-Host "[CRIT ] $msg"; Set-Severity 2 }
function Ok([string]$msg)    { Write-Host "[ OK  ] $msg" }

$curlArgs = @('-sk', '-m', '10')

# 1. Health readiness.
$ready = & curl.exe @curlArgs -o NUL -w "%{http_code}" "$BaseUrl/health/ready"
if ($ready -eq '200') { Ok "health/ready = $ready" } else { Crit "health/ready returned $ready" }
$live = & curl.exe @curlArgs -o NUL -w "%{http_code}" "$BaseUrl/health/live"
if ($live -eq '200') { Ok "health/live = $live" } else { Crit "health/live returned $live" }

# 2. PostgreSQL reachable (via docker exec in this topology; adapt for bare metal).
$pgOk = docker exec laohr-prodlike-postgres pg_isready -U laohr -d laohr 2>$null
if ($LASTEXITCODE -eq 0) { Ok "postgres: $pgOk" } else { Crit "postgres unreachable" }

# 3. Disk usage of the drive hosting Docker data + backups.
$dataDrive = Get-PSDrive -Name C
$usedPct = [math]::Round((($dataDrive.Used) / ($dataDrive.Used + $dataDrive.Free)) * 100)
if ($usedPct -ge $DiskCritPct) { Crit "disk ${usedPct}% used (>= $DiskCritPct%)" }
elseif ($usedPct -ge $DiskWarnPct) { Warn "disk ${usedPct}% used (>= $DiskWarnPct%)" }
else { Ok "disk ${usedPct}% used" }

# 4. Document storage writable? (API container volume)
$docProbe = docker exec laohr-prodlike-api sh -c 'touch /data/documents/.write-probe && rm /data/documents/.write-probe && echo WRITABLE' 2>$null
if ($docProbe -match 'WRITABLE') { Ok "document storage writable" } else { Crit "document storage unavailable/unwritable" }

# 5. Excessive 5xx: sample recent API logs.
$logErrs = docker logs --since 10m laohr-prodlike-api 2>&1 | Select-String ' responded 5\d\d ' | Measure-Object | Select-Object -ExpandProperty Count
if ($logErrs -gt 20) { Crit "$logErrs 5xx responses in last 10m (> 20)" }
elseif ($logErrs -gt 0) { Warn "$logErrs 5xx responses in last 10m" }
else { Ok "no 5xx responses in last 10m" }

# 6. API memory.
$apiMemLine = docker stats --no-stream --format "{{.Name}} {{.MemUsage}}" | Select-String 'laohr-prodlike-api'
if ($apiMemLine -match 'laohr-prodlike-api\s+([\d.]+)([MG]iB)') {
    $val = [double]$matches[1]; if ($matches[2] -eq 'GiB') { $val *= 1024 }
    if ($val -gt $ApiMemWarnMb) { Warn ("api memory {0:N0} MiB > {1} MiB" -f $val, $ApiMemWarnMb) }
    else { Ok ("api memory {0:N0} MiB" -f $val) }
}

# 7. DB connection saturation (max_connections vs current).
$pgConns = docker exec laohr-prodlike-postgres psql -U laohr -d laohr -t -A -c `
    "SELECT count(*) * 100 / current_setting('max_connections')::int FROM pg_stat_activity WHERE datname='laohr';" 2>$null
if ($pgConns -match '\d+') {
    $pct = [int]$matches[0]
    if ($pct -ge $PgConnSaturationPct) { Warn "db connections at $pct% of max_connections" }
    else { Ok "db connections at $pct% of max_connections" }
}

# 8. Backup freshness (newest *.dump under backup dir).
$newest = Get-ChildItem -Path $BackupDir -Filter '*.dump' -Recurse -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($newest) {
    $ageH = ((Get-Date) - $newest.LastWriteTime).TotalHours
    if ($ageH -gt $BackupMaxAgeHours) { Warn ("last backup {0:N1}h ago ({1})" -f $ageH, $newest.Name) }
    else { Ok ("last backup {0:N1}h ago" -f $ageH) }
} else { Warn "no backup file found under '$BackupDir'" }

exit $script:worst
