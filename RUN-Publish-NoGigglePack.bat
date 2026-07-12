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

echo ======================================================================
echo Publish run: No GigglePack Version / No Discord Message
echo ======================================================================
echo.
echo This runs the FULL publish pipeline EXCEPT GigglePack version bumping
echo and Discord update messages. Specifically:
echo.
echo   [YES] Sync Draft ^<-^> Game (pull game changes into workspace)
echo   [YES] Promote ActiveBuild -^> ReleaseSource (images + readme regeneration)
echo   [YES] Create all mod/category zips
echo   [YES] Generate thumbnails
echo   [YES] Update main README.md
echo.
echo   [NO]  GigglePack version bump (version stays unchanged)
echo   [NO]  Discord GigglePack release message
echo.
echo Use this when you want to re-publish existing content without triggering
echo a new GigglePack release or Discord notification.
echo.
echo ======================================================================
echo.
"%PYTHON_EXE%" "%~dp0SCRIPT-Main.py" --mode full --publish-gigglepack-action queue %*
set "EXIT_CODE=%ERRORLEVEL%"

echo.
if %EXIT_CODE% EQU 0 (
    echo No-GigglePack publish completed successfully.
    echo GigglePack version and Discord message were NOT updated.
) else (
    echo No-GigglePack publish exited with code %EXIT_CODE%.
    echo Check the log file for details.
)
echo.
pause
exit /b %EXIT_CODE%