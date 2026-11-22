param(
    [Parameter(Mandatory = $true)]
    [string]$BackupFile,
    [string]$ContainerName = "language_learning_mysql",
    [string]$Database = "language_learning_db",
    [string]$User = "root",
    [string]$Password = "root_password_123"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $BackupFile)) {
    throw "Backup file not found: $BackupFile"
}

$hashFile = "${BackupFile}.sha256"
if (Test-Path $hashFile) {
    $expectedHash = ((Get-Content $hashFile -First 1) -split "\s+")[0].Trim()
    $actualHash = (Get-FileHash -Path $BackupFile -Algorithm SHA256).Hash
    if ($expectedHash -ne $actualHash) {
        throw "Backup checksum mismatch! Expected $expectedHash, got $actualHash."
    }
    Write-Host "Checksum verified: $actualHash"
}

Write-Host "Restoring database '$Database' into container '$ContainerName' from '$BackupFile'..."
Get-Content -Path $BackupFile -Raw | docker exec -i $ContainerName mysql -u $User "-p$Password" $Database
if ($LASTEXITCODE -ne 0) {
    throw "mysql restore failed with exit code $LASTEXITCODE"
}

Write-Host "Restore completed successfully without data loss."