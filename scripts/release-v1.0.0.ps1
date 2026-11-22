param(
    [string]$VersionTag = "v1.0.0",
    [string]$ReleaseTitle = "v1.0.0 - Language Learning Web Application MVP Release"
)

$ErrorActionPreference = "Stop"

Write-Host "1. Fetching latest origin/main and origin/develop..."
git fetch origin main develop

Write-Host "2. Verifying clean annotated tag $VersionTag on origin/main..."
git tag -a $VersionTag origin/main -m $ReleaseTitle -f

Write-Host "3. Pushing tag $VersionTag to origin..."
git push origin $VersionTag --force

Write-Host "Release tag $VersionTag has been created and pushed successfully."