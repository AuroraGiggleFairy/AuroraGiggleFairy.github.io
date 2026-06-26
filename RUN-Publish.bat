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

echo Running publish run: update + promote + zips + README.
echo GigglePack publish gate will ask: finalize now or queue pending changes.
echo.
"%PYTHON_EXE%" "%~dp0SCRIPT-Main.py" --mode full --publish-gigglepack-action ask %*
set "EXIT_CODE=%ERRORLEVEL%"

echo.
echo Publish run exited with code %EXIT_CODE%.
pause
exit /b %EXIT_CODE%
