@echo off
setlocal

set "SITE_ROOT=%~dp0"
set "WORKFLOW=%SITE_ROOT%Workflow"
set "REPO_ROOT=%SITE_ROOT%..\.."
cd /d "%REPO_ROOT%"

set "PYTHON_EXE=%REPO_ROOT%\.venv\Scripts\python.exe"
set "PYTHONDONTWRITEBYTECODE=1"

if not exist "%PYTHON_EXE%" (
    echo ERROR: Python environment not found at "%PYTHON_EXE%"
    echo.
    pause
    exit /b 1
)

echo ============================================================
echo  7DAYSTODIEMODS GITHUB UPDATE
echo  Creates missing satellite repos and missing GitHub Releases.
echo  Does not change site descriptions, images, or titles.
echo  Does not run on every git push.
echo ============================================================
echo.
echo Optional first argument: single ReleaseSource base name.
echo Example: RUN-7DaysToDieMods-Update.bat AGF-BackpackPlus-072Slots
echo.

if "%~1"=="" (
    set /p CONFIRM=Create missing repos/Releases for ALL ReleaseSource mods? [y/N]:
) else (
    set /p CONFIRM=Create missing repo/Release for ONLY "%~1"? [y/N]:
)
if /I not "%CONFIRM%"=="y" (
    echo Cancelled. No GitHub changes.
    pause
    exit /b 0
)

echo.
echo Running validation first...
if "%~1"=="" (
    "%PYTHON_EXE%" "%WORKFLOW%\SCRIPT-7DaysToDieModsUpdate.py" --dry-run
) else (
    "%PYTHON_EXE%" "%WORKFLOW%\SCRIPT-7DaysToDieModsUpdate.py" --dry-run --mod "%~1"
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
    "%PYTHON_EXE%" "%WORKFLOW%\SCRIPT-7DaysToDieModsUpdate.py"
) else (
    "%PYTHON_EXE%" "%WORKFLOW%\SCRIPT-7DaysToDieModsUpdate.py" --mod "%~1"
)
set "EXIT_CODE=%ERRORLEVEL%"

echo.
if "%EXIT_CODE%"=="0" (
    echo Live update finished.
) else (
    echo Live update finished with errors. Exit code: %EXIT_CODE%
)
pause
exit /b %EXIT_CODE%
