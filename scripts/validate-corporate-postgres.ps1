# LaoHR / Lao Back Office — Corporate Operations PostgreSQL 16 validation (NO CI/CD)
# Starts a disposable postgres:16-alpine container, applies migrations, runs the
# real-relational concurrency integration tests (finance + corporate booking races),
# prints PASS/FAIL, then destroys the container.
#
# Local/manual only. No cloud runner.

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

$env:DOTNET_ROOT = "$HOME\.dotnet"
$env:Path = "$HOME\.dotnet;" + $env:Path

$container = "laohr-pg16-corporate-validate"
$port = 5436

Write-Host "=== Starting disposable PostgreSQL 16 ==="
docker rm -f $container 2>$null | Out-Null
docker run -d --name $container `
    -e POSTGRES_DB=laohr -e POSTGRES_USER=laohr -e POSTGRES_PASSWORD=laohr `
    -p "127.0.0.1:${port}:5432" postgres:16-alpine | Out-Null

try {
    $ready = $false
    for ($i = 0; $i -lt 30; $i++) {
        $out = docker exec $container pg_isready -U laohr -d laohr 2>&1
        if ($out -match "accepting connections") { $ready = $true; break }
        Start-Sleep -Seconds 1
    }
    if (-not $ready) { throw "PostgreSQL 16 did not become ready." }

    Write-Host "=== Applying migrations ==="
    $env:NPGSQL_LEGACY_TIMESTAMP = "1"
    Push-Location "$root\Backend\LaoHR.API"
    dotnet ef database update --connection "Host=127.0.0.1;Port=$port;Database=laohr;Username=laohr;Password=laohr" | Out-Null
    Pop-Location

    Write-Host "=== Running real-relational concurrency tests ==="
    $env:LAOHR_TEST_CONNECTION = "Host=127.0.0.1;Port=$port;Database=laohr;Username=laohr;Password=laohr"
    Push-Location "$root\Backend\LaoHR.Tests"
    dotnet test --no-restore --filter "FullyQualifiedName~Pg16Concurrency" 2>&1 | Select-String "Passed!|Failed!|error"
    Pop-Location
}
finally {
    Write-Host "=== Destroying container ==="
    docker rm -f $container 2>$null | Out-Null
}
