@echo off
setlocal

cd /d "%~dp0"

set "PYTHON_EXE=%~dp0.venv\Scripts\python.exe"
set "SCRIPT=%~dp000_Support\Automation\mod-tools\SCRIPT-MakeNewMod.py"

if not exist "%PYTHON_EXE%" (
    echo ERROR: Python environment not found at "%PYTHON_EXE%"
    echo Run the workspace setup or recreate the .venv before using this launcher.
    echo.
    pause
    exit /b 1
)

if not exist "%SCRIPT%" (
    echo ERROR: MakeNewMod script not found at "%SCRIPT%"
    echo.
    pause
    exit /b 1
)

echo Creating a new Draft mod scaffold...
echo.
"%PYTHON_EXE%" "%SCRIPT%" %*
set "EXIT_CODE=%ERRORLEVEL%"

echo.
if %EXIT_CODE% EQU 0 (
    echo MakeNewMod completed successfully.
) else (
    echo MakeNewMod exited with code %EXIT_CODE%.
)
echo.
pause
exit /b %EXIT_CODE%
