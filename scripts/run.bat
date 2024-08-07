# Stop execution on any error
$ErrorActionPreference = "Stop"

# Navigate to the script directory
Set-Location -Path (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

# Start MySQL service
docker-compose up -d mysql

# Wait for MySQL to be ready
Write-Output "Waiting for MySQL to be ready (30 seconds)..."
Start-Sleep -Seconds 30

# Start all services
Write-Output "Starting all services..."
docker-compose up --build

# # Apply migrations for TransactionsService
# Write-Output "Applying migrations for TransactionsService..."
# docker-compose run --rm transactions-service dotnet ef database update --project src/TransactionsService

# # Apply migrations for DailySummaryService
# Write-Output "Applying migrations for DailySummaryService..."
# docker-compose run --rm daily-summary-service dotnet ef database update --project src/DailySummaryService
