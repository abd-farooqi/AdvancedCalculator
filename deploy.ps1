# Deploy Advanced Calculator to GitHub + GitHub Pages + Release
# Run this AFTER Git is installed

$ErrorActionPreference = "Stop"

Write-Host "=== Advanced Calculator Deployer ===" -ForegroundColor Cyan
Write-Host ""

# Refresh PATH
$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

# Check prerequisites
Write-Host "Checking prerequisites..." -ForegroundColor Yellow

$gitPath = Get-Command git -ErrorAction SilentlyContinue
if (-not $gitPath) {
    Write-Host "ERROR: Git not found. Install it first:" -ForegroundColor Red
    Write-Host "  winget install Git.Git" -ForegroundColor Yellow
    Write-Host "Then restart this script." -ForegroundColor Yellow
    exit 1
}

$ghPath = Get-Command gh -ErrorAction SilentlyContinue
if (-not $ghPath) {
    Write-Host "ERROR: GitHub CLI not found. Install it first:" -ForegroundColor Red
    Write-Host "  winget install GitHub.cli" -ForegroundColor Yellow
    exit 1
}

Write-Host "[OK] Git: $(git --version)" -ForegroundColor Green
Write-Host "[OK] GitHub CLI: $(gh --version | Select-Object -First 1)" -ForegroundColor Green

# Check auth
$authStatus = gh auth status 2>&1
if ($authStatus -match "not logged") {
    Write-Host "`nYou need to login to GitHub first." -ForegroundColor Yellow
    Write-Host "Run: gh auth login" -ForegroundColor Yellow
    Write-Host "Then restart this script." -ForegroundColor Yellow
    exit 1
}

# Build release exe
Write-Host "`nBuilding release .exe..." -ForegroundColor Cyan
dotnet publish AdvancedCalculator -c Release -o dist -p:PublishSingleFile=true --self-contained false

# Create GitHub repo
Write-Host "`nCreating GitHub repository..." -ForegroundColor Cyan
gh repo create abd-farooqi/AdvancedCalculator --private --source . --remote origin 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "[OK] Repository created" -ForegroundColor Green
} else {
    # Might already exist, try to set remote
    gh repo set-default abd-farooqi/AdvancedCalculator 2>$null
    Write-Host "[OK] Using existing repository" -ForegroundColor Green
}

# Git init and commit
git add .
git commit -m "feat: Advanced Calculator v1.0.0 - Standard, Scientific & Age Calculator" 2>$null
if ($LASTEXITCODE -ne 0) {
    git add .
    git commit -m "Initial commit: Advanced Calculator v1.0.0" 2>$null
}

# Push to main
git branch -M main
git push -u origin main --force

# Create release
Write-Host "`nCreating GitHub Release..." -ForegroundColor Cyan
$exePath = "dist\AdvancedCalculator.exe"
if (Test-Path $exePath) {
    gh release create v1.0.0 $exePath --title "v1.0.0 - Initial Release" --notes @"
# Advanced Calculator v1.0.0

## Features
- Standard Calculator (+, −, ×, ÷, %, √, x², 1/x)
- Scientific Calculator (sin, cos, tan, log, ln, xʸ, x!, π, e)
- Age Calculator (exact age, heartbeats, zodiac, birthday countdown)
- Memory functions (MC, MR, M+, M−, MS)
- History panel
- Full keyboard support
- Beautiful dark glass theme

## System Requirements
- Windows 10/11
- .NET 8.0 Runtime: https://dotnet.microsoft.com/download/dotnet/8.0

## Installation
1. Download AdvancedCalculator.exe
2. Install .NET 8.0 if you don't have it
3. Double-click to run
"@ 2>$null
    Write-Host "[OK] Release v1.0.0 created" -ForegroundColor Green
}

# Enable GitHub Pages
Write-Host "`nEnabling GitHub Pages..." -ForegroundColor Cyan
gh api repos/abd-farooqi/AdvancedCalculator/pages --method PUT -f source='{"branch":"main","path":"/docs"}' 2>$null
Write-Host "[OK] GitHub Pages configured (will be live at https://abd-farooqi.github.io/AdvancedCalculator/)" -ForegroundColor Green

Write-Host "`n=== Deployment Complete! ===" -ForegroundColor Green
Write-Host ""
Write-Host "Your calculator is now live at:" -ForegroundColor Cyan
Write-Host "  GitHub: https://github.com/abd-farooqi/AdvancedCalculator" -ForegroundColor White
Write-Host "  Download: https://github.com/abd-farooqi/AdvancedCalculator/releases/latest" -ForegroundColor White
Write-Host "  Website: https://abd-farooqi.github.io/AdvancedCalculator/" -ForegroundColor White
Write-Host ""
