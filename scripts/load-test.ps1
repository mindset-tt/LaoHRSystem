# LaoHR Phase 4D.1 — LOAD TEST ORCHESTRATOR (NO CI/CD)
#
# Runs k6 against the production-like stack at escalating concurrency
# levels while sampling container resources and PostgreSQL activity.
#
# INTERNAL_ENGINEERING_PROFILE — not a business SLA. Disposable DB only.
#
# Usage (from repo root):
#   .\scripts\load-test.ps1 -Levels 10,25,50,100 -DurationPerLevel 120
#
# Requires: k6.exe path via $env:K6_EXE, stack running via docker-compose.prodlike.yml,
# POSTGRES_PASSWORD in $env:POSTGRES_PASSWORD for pg_stat_activity sampling.

param(
    [int[]]$Levels = @(10, 25, 50, 100),
    [int]$DurationPerLevel = 120,          # seconds of sustained load per level
    [string]$OutDir = "docs\production-phase-4d.1\evidence\load",
    [string]$BaseUrl = "https://localhost"
)

$ErrorActionPreference = 'Stop'
$k6 = $env:K6_EXE
if (-not $k6) { throw "Set `$env:K6_EXE to the k6 executable path." }

New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'

function Sample-Resources {
    param([string]$Tag, [string]$Path)
    $stats = docker stats --no-stream --format "{{.Name}} cpu={{.CPUPerc}} mem={{.MemUsage}}" 2>$null |
        Where-Object { $_ -match 'laohr-prodlike' }
    $db = ''
    if ($env:POSTGRES_PASSWORD) {
        $db = docker exec laohr-prodlike-postgres psql -U laohr -d laohr -t -A -c `
            "SELECT 'pg_active=' || count(*) FILTER (WHERE state = 'active') || ' pg_idle=' || count(*) FILTER (WHERE state = 'idle') || ' pg_total=' || count(*) FROM pg_stat_activity WHERE datname = 'laohr';" 2>$null
    }
    "$Tag|$stats|$db" | Add-Content -Path $Path
}

foreach ($vus in $Levels) {
    Write-Host "`n=== LOAD LEVEL: $vus concurrent users / ${DurationPerLevel}s ==="
    $summary = Join-Path $OutDir "load-$vus-vus-$stamp.json"
    $samples = Join-Path $OutDir "resources-$vus-vus-$stamp.txt"

    # Start background resource sampler (every 5s).
    $job = Start-Job -ScriptBlock {
        param($samples)
        while ($true) {
            $stats = docker stats --no-stream --format "{{.Name}} cpu={{.CPUPerc}} mem={{.MemUsage}}" 2>$null |
                Where-Object { $_ -match 'laohr-prodlike' }
            $db = docker exec laohr-prodlike-postgres psql -U laohr -d laohr -t -A -c "SELECT 'pg_active=' || count(*) FILTER (WHERE state='active') || ' pg_idle=' || count(*) FILTER (WHERE state='idle') || ' pg_total=' || count(*) FROM pg_stat_activity WHERE datname='laohr';" 2>$null
            "$(Get-Date -Format o)|$stats|$db" | Add-Content -Path $samples
            Start-Sleep -Seconds 5
        }
    } -ArgumentList $samples

    & $k6 run `
        -e BASE_URL=$BaseUrl `
        -e VUS=$vus `
        -e DURATION="$($DurationPerLevel)s" `
        --summary-export $summary `
        scripts\load-test.js 2>&1 | Tee-Object -FilePath (Join-Path $OutDir "load-$vus-vus-$stamp.log") | Select-Object -Last 12

    Stop-Job $job; Remove-Job $job -Force

    # Cooldown so the next level starts from a quiet system.
    Start-Sleep -Seconds 20
}

Write-Host "`n=== All levels complete. Results in $OutDir ==="
