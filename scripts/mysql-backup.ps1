param(
    [string]$ContainerName = "language_learning_mysql",
    [string]$Database = "language_learning_db",
    [string]$User = "root",
    [string]$Password = "root_password_123",
    [string]$OutputDir = "./backups"
)

$ErrorActionPreference = "Stop"
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

$timestamp = (Get-Date).ToUniversalTime().ToString("yyyyMMdd-HHmmss")
$backupFile = Join-Path $OutputDir "${Database}_backup_${timestamp}.sql"
$hashFile = "${backupFile}.sha256"

Write-Host "Creating MySQL backup for database '$Database' from container '$ContainerName'..."
docker exec $ContainerName mysqldump -u $User "-p$Password" --single-transaction --routines --triggers --databases $Database | Out-File -FilePath $backupFile -Encoding utf8
if ($LASTEXITCODE -ne 0) {
    throw "mysqldump failed with exit code $LASTEXITCODE"
}

$hash = (Get-FileHash -Path $backupFile -Algorithm SHA256).Hash
Set-Content -Path $hashFile -Value "$hash  $(Split-Path $backupFile -Leaf)" -Encoding utf8

Write-Host "Backup completed: $backupFile (SHA256: $hash)"
Write-Output $backupFile