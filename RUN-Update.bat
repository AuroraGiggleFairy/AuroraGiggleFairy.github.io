@echo off
setlocal

cd /d "%~dp0"

set "PYTHON_EXE=%~dp0.venv\Scripts\python.exe"

if not exist "%PYTHON_EXE%" (
    echo ERROR: Python environment not found at "%PYTHON_EXE%"
    echo Run the workspace setup or recreate the .venv before using this launcher.
    echo.
    pause
    exit /b 1
)

echo Running update run: sync, drift resolution, CSV reconcile.
echo.
"%PYTHON_EXE%" "%~dp000_Support\Automation\workflow\00_dispatch.py" --mode update %*
set "EXIT_CODE=%ERRORLEVEL%"

if %EXIT_CODE% equ 0 (
    echo.
    echo Syncing Publishing Matrix Canvas...
    "%PYTHON_EXE%" "%~dp006_PublishingSupport\SCRIPT-SyncPublishingMatrix.py"
)

echo.
echo Update run exited with code %EXIT_CODE%.
pause
exit /b %EXIT_CODE%
