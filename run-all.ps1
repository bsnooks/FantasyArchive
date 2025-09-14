#!/usr/bin/env pwsh
# run-all.ps1
# Script to run frontend for FantasyArchive

Write-Host "Fantasy Archive - Starting Development Environment" -ForegroundColor Green
Write-Host "======================================================" -ForegroundColor Green

# Start frontend
Write-Host "Starting React frontend..." -ForegroundColor Yellow
Set-Location "frontend"
Start-Process -NoNewWindow -FilePath "npm" -ArgumentList "start" -PassThru
Set-Location ".."

Write-Host ""
Write-Host "Frontend started! It should open in your browser shortly." -ForegroundColor Green
Write-Host ""
Write-Host "Available commands:" -ForegroundColor Cyan
Write-Host "  To run the data exporter:" -ForegroundColor White
Write-Host "    cd exporter && dotnet run" -ForegroundColor Gray
Write-Host ""
Write-Host "  To stop all services:" -ForegroundColor White
Write-Host "    Press Ctrl+C in this window" -ForegroundColor Gray