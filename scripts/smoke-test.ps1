param(
    [string]$ApiBaseUrl = "http://localhost:5000",
    [string]$WebBaseUrl = "http://localhost:3000"
)

$ErrorActionPreference = "Stop"

Write-Host "1. Checking API & Database Health at $ApiBaseUrl/health ..."
$health = Invoke-RestMethod -Uri "$ApiBaseUrl/health" -Method Get -TimeoutSec 10
if ($health.status -ne "Healthy" -or $health.database -ne "Healthy") {
    throw "Health check failed: $($health | ConvertTo-Json -Compress)"
}
Write-Host "   [OK] API Status=$($health.api), Database=$($health.database)"

Write-Host "2. Checking versioned health endpoint at $ApiBaseUrl/api/v1/health ..."
$v1Health = Invoke-RestMethod -Uri "$ApiBaseUrl/api/v1/health" -Method Get -TimeoutSec 10
if ($v1Health.status -ne "Healthy") {
    throw "Versioned health check failed."
}
Write-Host "   [OK] Versioned health endpoint reachable."

Write-Host "3. Checking Frontend Web App at $WebBaseUrl ..."
$webResponse = Invoke-WebRequest -Uri $WebBaseUrl -Method Get -TimeoutSec 10 -UseBasicParsing
if ($webResponse.StatusCode -ne 200) {
    throw "Frontend smoke check returned status $($webResponse.StatusCode)"
}
Write-Host "   [OK] Frontend Web App returned HTTP 200."
Write-Host "Staging smoke test completed successfully."