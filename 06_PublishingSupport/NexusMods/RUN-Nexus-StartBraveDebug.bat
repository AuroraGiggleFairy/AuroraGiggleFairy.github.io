@echo off
setlocal
cd /d "%~dp0"

echo === Start Brave with Remote Debugging (port 9222) ===
echo.
echo IMPORTANT:
echo 1. Close ALL Brave windows first (check system tray for background apps).
echo 2. Once Brave opens, log into Nexus Mods if not already logged in (using 1Password).
echo 3. Keep Brave open while running Playwright automation.
echo.
pause

set "PYTHON_EXE=%~dp0..\..\.venv\Scripts\python.exe"
if not exist "%PYTHON_EXE%" (
    set "PYTHON_EXE=python"
)

"%PYTHON_EXE%" Workflow\SCRIPT-NexusPlaywright.py start-brave-debug
if errorlevel 1 (
    echo.
    echo FAILED to start Brave on port 9222.
    echo Ensure Brave is completely closed and try again.
    pause
    exit /b 1
)

echo.
echo Brave is running and listening on port 9222.
pause
