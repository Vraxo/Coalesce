# publish.ps1
# Interactive publication utility script for Coalesce

# 1. Automatically detect the host OS to set target runtime identifier (RID)
$Runtime = "win-x64"
if ($IsMacOS) { 
    $Runtime = "osx-arm64" 
}
elseif ($IsLinux) { 
    $Runtime = "linux-x64" 
}

# 2. Render clean, minimal menu
Write-Host "`n--- COALESCE PUBLISH UTILITY ---" -ForegroundColor Cyan
Write-Host "1) Portable (Framework-Dependent)"
Write-Host "2) Standalone (Self-Contained & Trimmed)"
Write-Host "3) Native AOT (Ahead-of-Time)"
Write-Host "4) All Configurations"
Write-Host ""

$Choice = Read-Host "Select option (1-4)"

# 3. Execute publish based on choice
switch ($Choice) {
    "1" {
        & dotnet publish -c Release -o ./artifacts/publish/portable
    }
    "2" {
        & dotnet publish -c Release -r $Runtime --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o ./artifacts/publish/self-contained
    }
    "3" {
        & dotnet publish -c Release -r $Runtime -p:PublishAot=true -o ./artifacts/publish/aot
    }
    "4" {
        Write-Host "`nPublishing all configurations..." -ForegroundColor Cyan
        & dotnet publish -c Release -o ./artifacts/publish/portable
        if ($LASTEXITCODE -eq 0) {
            & dotnet publish -c Release -r $Runtime --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o ./artifacts/publish/self-contained
        }
        if ($LASTEXITCODE -eq 0) {
            & dotnet publish -c Release -r $Runtime -p:PublishAot=true -o ./artifacts/publish/aot
        }
    }
    Default {
        Write-Host "Invalid choice. Canceled." -ForegroundColor Red
        Exit 1
    }
}

# 4. Display result summary
if ($LASTEXITCODE -eq 0) {
    Write-Host "`nPublish completed successfully!" -ForegroundColor Green
} else {
    Write-Host "`nPublish failed!" -ForegroundColor Red
    Exit 1
}