@echo off
setlocal
cd /d "%~dp0"

set "PYTHON_EXE=python"
if exist "..\.venv\Scripts\python.exe" (
    set "PYTHON_EXE=..\.venv\Scripts\python.exe"
)

echo ============================================================
echo   PHASE 1: NEXUS MODS BATCH CREATE
echo ============================================================
%PYTHON_EXE% NexusMods\Workflow\SCRIPT-Nexus-BatchCreate.py --publish
if errorlevel 1 (
    echo.
    echo Nexus batch create encountered an error. Proceeding to 7daystodiemods...
)

echo.
echo ============================================================
echo   PHASE 2: 7DAYSTODIEMODS BATCH CREATE
echo ============================================================
%PYTHON_EXE% 7DaysToDieMods\Workflow\SCRIPT-7dtdmods-BatchCreate.py --publish
if errorlevel 1 (
    echo.
    echo 7daystodiemods batch create encountered an error.
    if not "%~1"=="--no-pause" pause
    exit /b 1
)

echo.
echo ============================================================
echo   ALL NEW MODS PUBLISHED
echo ============================================================
endlocal
