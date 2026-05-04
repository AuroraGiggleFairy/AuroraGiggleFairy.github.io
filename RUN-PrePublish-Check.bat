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

echo Running pre-publish confidence checks:
echo   1) self-test harness
echo   2) update dry-run with verbose output
echo.

"%PYTHON_EXE%" "%~dp0SCRIPT-Main.py" --mode self-test %*
if errorlevel 1 (
    echo.
    echo Pre-publish check failed during self-test.
    pause
    exit /b 1
)

"%PYTHON_EXE%" "%~dp0SCRIPT-Main.py" --mode update --dry-run --verbose %*
set "EXIT_CODE=%ERRORLEVEL%"

echo.
echo Pre-publish checks exited with code %EXIT_CODE%.
pause
exit /b %EXIT_CODE%
