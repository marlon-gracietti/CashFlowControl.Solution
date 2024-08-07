@echo off
setlocal enabledelayedexpansion

REM Navigate to the script directory
cd /d "%~dp0"

REM Start MySQL service
docker-compose up -d mysql

REM Wait for MySQL to be ready
echo Waiting for MySQL to be ready (30 seconds)...
timeout /t 30 /nobreak

REM Start all services
echo Starting all services...
docker-compose up --build

REM Apply migrations for TransactionsService
REM echo Applying migrations for TransactionsService...
REM docker-compose run --rm transactionsservice dotnet ef database update --project src/TransactionsService

REM Apply migrations for DailySummaryService
REM echo Applying migrations for DailySummaryService...
REM docker-compose run --rm dailysummaryservice dotnet ef database update --project src/DailySummaryService

endlocal
