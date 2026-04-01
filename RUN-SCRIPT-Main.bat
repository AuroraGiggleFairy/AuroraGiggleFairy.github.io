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

echo Running SCRIPT-Main.py %*
echo.
"%PYTHON_EXE%" "%~dp0SCRIPT-Main.py" %*
set "EXIT_CODE=%ERRORLEVEL%"

echo.
echo SCRIPT-Main.py exited with code %EXIT_CODE%.
pause
exit /b %EXIT_CODE%