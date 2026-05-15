@echo off
setlocal enabledelayedexpansion

:: ===============================
:: SAFE GIT + BACKUP SCRIPT
:: ===============================

echo =====================================
echo   PROJECT SAFE BACKUP SCRIPT
echo =====================================

:: Get timestamp
for /f "tokens=1-3 delims=/. " %%a in ("%date%") do (
    set DATE=%%c-%%b-%%a
)
for /f "tokens=1-3 delims=:. " %%a in ("%time%") do (
    set TIME=%%a-%%b
)

set BACKUP_NAME=backup_%DATE%_%TIME%
set BACKUP_NAME=%BACKUP_NAME: =0%

echo Creating ZIP backup: %BACKUP_NAME%.zip

:: Create backup zip using PowerShell
powershell -Command "Compress-Archive -Path .\* -DestinationPath .\%BACKUP_NAME%.zip -Force"

echo Backup created.

:: Git operations
echo Adding changes...
git add .

set /p msg=Enter commit message: 

if "%msg%"=="" set msg=auto backup commit

git commit -m "%msg%"

echo Pushing to origin...
git push

echo =====================================
echo DONE. Project safely backed up.
echo =====================================

pause
