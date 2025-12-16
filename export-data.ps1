#!/usr/bin/env pwsh
# export-data.ps1
# Script to run the data exporter and update frontend data

Write-Host "Fantasy Archive - Data Exporter" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green

Write-Host "Exporting data to frontend..." -ForegroundColor Yellow

Set-Location "exporter"
dotnet run
Set-Location ".."

Write-Host ""
Write-Host "Copying exported data to frontend..." -ForegroundColor Yellow

# Copy all exported data to frontend public directory
$sourceDir = "exporter/exports"
$targetDir = "frontend/public/data"

if (Test-Path $sourceDir) {
    # Remove existing data directory to ensure clean copy
    if (Test-Path $targetDir) {
        Remove-Item $targetDir -Recurse -Force
        Write-Host "Removed existing frontend data directory" -ForegroundColor Gray
    }
    
    # Copy all exported data
    Copy-Item $sourceDir $targetDir -Recurse -Force
    Write-Host "Copied all exported data to frontend" -ForegroundColor Green
} else {
    Write-Host "Warning: Export directory not found at $sourceDir" -ForegroundColor Red
}

Write-Host ""
Write-Host "Data export completed!" -ForegroundColor Green
Write-Host "The frontend data has been updated with the latest exports." -ForegroundColor Green
Write-Host "Your new weekly matchup records are now available:" -ForegroundColor Cyan
Write-Host "  • Lowest Winning Scores" -ForegroundColor White
Write-Host "  • Highest Losing Scores" -ForegroundColor White
Write-Host "  • Smallest Victory Margins" -ForegroundColor White  
Write-Host "  • Largest Victory Margins" -ForegroundColor White
Write-Host ""