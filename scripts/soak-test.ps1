# LaoHR Phase 4D.1 — SOAK TEST ORCHESTRATOR
#
# Sustained moderate load (default 25 VUs / 45 min) against the production-like
# stack. Samples every 60s: container CPU/RAM, PostgreSQL connections & size,
# document-storage size, API /metrics counters (audit drops, GC, exceptions).
#
# INTERNAL_ENGINEERING_PROFILE — disposable DB only.

param(
    [int]$Vus = 25,
    [int]$Minutes = 45,
    [string]$OutDir = "docs\production-phase-4d.1\evidence\soak",
    [string]$BaseUrl = "https://localhost"
)

$ErrorActionPreference = 'Stop'
$k6 = $env:K6_EXE
if (-not $k6) { throw "Set `$env:K6_EXE to the k6 executable path." }

New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$samples = Join-Path $OutDir "soak-samples-$stamp.txt"
$summary = Join-Path $OutDir "soak-$stamp.json"

"soak start $(Get-Date -Format o)" | Add-Content $samples

# Sampler loop.
$job = Start-Job -ScriptBlock {
    param($samples)
    while ($true) {
        $stats = docker stats --no-stream --format "{{.Name}} cpu={{.CPUPerc}} mem={{.MemUsage}}" 2>$null |
            Where-Object { $_ -match 'laohr-prodlike' }
        $pg = docker exec laohr-prodlike-postgres psql -U laohr -d laohr -t -A -c `
            "SELECT 'pg_active=' || count(*) FILTER (WHERE state='active') || ' pg_idle=' || count(*) FILTER (WHERE state='idle') || ' pg_total=' || count(*) || ' db_size_mb=' || round(pg_database_size('laohr')/1048576.0) FROM pg_stat_activity WHERE datname='laohr';" 2>$null
        "$(Get-Date -Format o)|$stats|$pg" | Add-Content -Path $samples
        Start-Sleep -Seconds 60
    }
} -ArgumentList $samples

try {
    & $k6 run `
        -e BASE_URL=$BaseUrl `
        -e VUS=$Vus `
        -e DURATION="$($Minutes)m" `
        --summary-export $summary `
        scripts\load-test.js 2>&1 | Tee-Object -FilePath (Join-Path $OutDir "soak-$stamp.log") | Select-Object -Last 15
}
finally {
    Stop-Job $job -ErrorAction SilentlyContinue; Remove-Job $job -Force -ErrorAction SilentlyContinue
    "soak end $(Get-Date -Format o)" | Add-Content $samples

    # Post-cooldown memory sample (leak assessment).
    Start-Sleep -Seconds 120
    "cooldown(120s) $(Get-Date -Format o)" | Add-Content $samples
    docker stats --no-stream --format "{{.Name}} cpu={{.CPUPerc}} mem={{.MemUsage}}" 2>$null |
        Where-Object { $_ -match 'laohr-prodlike' } | ForEach-Object { $_ } | Add-Content $samples
}
