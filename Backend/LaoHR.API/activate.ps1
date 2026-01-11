$key = Get-Content ..\LaoHR.LicenseGen\license.key -Raw
$key = $key.Trim()
Write-Host "Sending Key: $key"
$body = @{ Key = $key } | ConvertTo-Json
Write-Host "Body: $body"
try {
    Invoke-RestMethod -Uri "http://localhost:5000/api/license/activate" -Method Post -Body $body -ContentType "application/json"
}
catch {
    Write-Host "Error: $_"
    $_.Exception.Response
}
