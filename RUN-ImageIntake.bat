@echo off
setlocal

cd /d "%~dp0"

set "PYTHON_EXE=%~dp0.venv\Scripts\python.exe"
set "SCRIPT=%~dp000_Images\SCRIPT-ImageIntake.py"

if not exist "%PYTHON_EXE%" (
    echo ERROR: Python environment not found at "%PYTHON_EXE%"
    echo Run the workspace setup or recreate the .venv before using this launcher.
    echo.
    pause
    exit /b 1
)

if not exist "%SCRIPT%" (
    echo ERROR: Image intake script not found at "%SCRIPT%"
    echo.
    pause
    exit /b 1
)

echo Running image intake...
echo.
"%PYTHON_EXE%" "%SCRIPT%" %*
set "EXIT_CODE=%ERRORLEVEL%"

echo.
echo Image intake exited with code %EXIT_CODE%.
pause
exit /b %EXIT_CODE%
