# LaoHR / Lao Back Office — production smoke suite (NO CI/CD)
# Read-only / safe checks against a running deployment. No financial writes.
# Usage: .\scripts\production-smoke.ps1 -BaseUrl http://localhost:8080

$ErrorActionPreference = 'Stop'

param(
    [string]$BaseUrl = "http://localhost:8080"
)

function Check($name, $scriptBlock) {
    try {
        & $scriptBlock | Out-Null
        Write-Host "PASS  $name"
    }
    catch {
        Write-Host "FAIL  $name : $($_.Exception.Message)"
    }
}

Write-Host "=== Production smoke: $BaseUrl ==="

Check "health/live" { Invoke-WebRequest -Uri "$BaseUrl/health/live" -UseBasicParsing | Out-Null }
Check "health/ready" { Invoke-WebRequest -Uri "$BaseUrl/health/ready" -UseBasicParsing | Out-Null }

# Unauthenticated protected API must return 401.
Check "unauthenticated /api/employees -> 401" {
    try {
        Invoke-WebRequest -Uri "$BaseUrl/api/employees" -UseBasicParsing | Out-Null
        throw "expected 401"
    }
    catch {
        if ($_.Exception.Response.StatusCode.value__ -ne 401) { throw "expected 401, got $($_.Exception.Response.StatusCode.value__)" }
    }
}

Write-Host "=== Smoke complete ==="
