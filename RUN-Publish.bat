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

echo ============================================================
echo  PUBLISH RUN
echo ============================================================
echo Running: update + promote + zips + README + GigglePack.
echo GigglePack publish gate will ask: finalize now, append to
echo latest finalize notes, or queue pending changes.
echo.
"%PYTHON_EXE%" "%~dp0SCRIPT-Main.py" --mode full --publish-gigglepack-action ask %*
set "EXIT_CODE=%ERRORLEVEL%"

if %EXIT_CODE% neq 0 (
    echo.
    echo Publish run finished with exit code %EXIT_CODE%.
    pause
    exit /b %EXIT_CODE%
)

echo.
echo ============================================================
echo  STEP 6 — Generating Nexus PublishHelp files
echo ============================================================
echo.
"%PYTHON_EXE%" "%~dp0Workflow\06_nexus.py"
set "EXIT_CODE=%ERRORLEVEL%"

echo.
echo Publish run exited with code %EXIT_CODE%.
pause
exit /b %EXIT_CODE%
