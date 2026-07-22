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
echo DRY RUN - Publish: No GigglePack Version / No Discord Message
echo ======================================================================
echo.
echo This runs the FULL publish pipeline in DRY-RUN mode EXCEPT GigglePack
echo version bumping and Discord update messages.
echo.
echo   [YES] SYNC Draft ^<-^> Game (pull game changes into workspace)
echo   [YES] Promote ActiveBuild -^> ReleaseSource (images + readme regeneration)
echo   [YES] Create all mod/category zips
echo   [YES] Generate thumbnails
echo   [YES] Update main README.md
echo.
echo   [NO]  GigglePack version bump (version stays unchanged)
echo   [NO]  Discord GigglePack release message
echo.
echo DRY-RUN: No files will be changed.
echo.
echo ======================================================================
echo.
"%PYTHON_EXE%" "%~dp0SCRIPT-Main.py" --mode full --dry-run --verbose --publish-gigglepack-action queue %*
set "EXIT_CODE=%ERRORLEVEL%"

echo.
echo Dry-run (no-GigglePack) exited with code %EXIT_CODE%.
pause
exit /b %EXIT_CODE%