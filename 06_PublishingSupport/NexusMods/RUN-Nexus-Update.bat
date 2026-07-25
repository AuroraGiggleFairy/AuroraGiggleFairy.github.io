@echo off
setlocal

set "NEXUS_ROOT=%~dp0"
set "WORKFLOW=%NEXUS_ROOT%Workflow"
set "REPO_ROOT=%NEXUS_ROOT%..\.."
cd /d "%REPO_ROOT%"

set "PYTHON_EXE=%REPO_ROOT%\.venv\Scripts\python.exe"
set "PYTHONDONTWRITEBYTECODE=1"

if not exist "%PYTHON_EXE%" (
    echo ERROR: Python environment not found at "%PYTHON_EXE%"
    echo.
    pause
    exit /b 1
)

set "API_KEY_FILE=%NEXUS_ROOT%nexus-api-key.private.txt"
if not exist "%API_KEY_FILE%" (
    echo Paste your Nexus API key into the Notepad window, save it, then close Notepad.
    echo.>"%API_KEY_FILE%"
    start /wait notepad.exe "%API_KEY_FILE%"
)
set /p AGF_NEXUSMODS_API_KEY=<"%API_KEY_FILE%"
if not defined AGF_NEXUSMODS_API_KEY (
    echo ERROR: No Nexus API key found in %API_KEY_FILE%
    pause
    exit /b 1
)

echo ============================================================
echo  NEXUS LIVE UPDATE
echo  Pushes newer zip versions to EXISTING Nexus mod pages.
echo  Cannot create new mod pages. Page body / images are not changed.
echo ============================================================
echo.
echo Recommended: run RUN-Nexus-Status.bat first.
echo Optional first argument: single mod base name for a pilot update.
echo Example: RUN-Nexus-Update.bat AGF-NoEAC-Toolbelt12Slots
echo.

if "%~1"=="" (
    set /p CONFIRM=Update ALL configured mods that are newer locally? [y/N]: 
) else (
    set /p CONFIRM=Update ONLY "%~1"? [y/N]: 
)
if /I not "%CONFIRM%"=="y" (
    echo Cancelled. No Nexus changes.
    pause
    exit /b 0
)

echo.
echo Running validation first...
if "%~1"=="" (
    "%PYTHON_EXE%" "%WORKFLOW%\SCRIPT-NexusUpdate.py" --only update --dry-run
) else (
    "%PYTHON_EXE%" "%WORKFLOW%\SCRIPT-NexusUpdate.py" --only update --dry-run --mod "%~1"
)
if errorlevel 1 (
    echo.
    echo Validation reported blockers. Aborting live update.
    pause
    exit /b 1
)

echo.
echo Validation OK. Starting live update...
echo.
if "%~1"=="" (
    "%PYTHON_EXE%" "%WORKFLOW%\SCRIPT-NexusUpdate.py" --only update
) else (
    "%PYTHON_EXE%" "%WORKFLOW%\SCRIPT-NexusUpdate.py" --only update --mod "%~1"
)
set "EXIT_CODE=%ERRORLEVEL%"

echo.
if "%EXIT_CODE%"=="0" (
    echo Live update finished.
    echo Re-run RUN-Nexus-Status.bat to refresh Nexus-Status.md.
) else (
    echo Live update finished with errors. Exit code: %EXIT_CODE%
)
pause
exit /b %EXIT_CODE%
