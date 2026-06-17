@echo off
setlocal

cd /d "%~dp0.."

set "PYTHON_EXE=%CD%\.venv\Scripts\python.exe"
if not exist "%PYTHON_EXE%" (
    echo ERROR: Python environment not found at "%PYTHON_EXE%"
    echo Create or restore .venv before running this launcher.
    echo.
    pause
    exit /b 1
)

"%PYTHON_EXE%" "%CD%\00_Images\SCRIPT-ImageIntake.py" %*
set "EXIT_CODE=%ERRORLEVEL%"

echo.
echo Image intake exited with code %EXIT_CODE%.
pause
exit /b %EXIT_CODE%
