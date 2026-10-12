@echo off
setlocal

set "NEXUS_ROOT=%~dp0"
set "WORKFLOW=%NEXUS_ROOT%Workflow"
set "REPO_ROOT=%NEXUS_ROOT%..\.."
cd /d "%REPO_ROOT%"

set "PYTHON_EXE=%REPO_ROOT%\.venv\Scripts\python.exe"
set "PYTHONDONTWRITEBYTECODE=1"

if not exist "%PYTHON_EXE%" (
    set "PYTHON_EXE=python"
)

echo ============================================================
echo  NEXUS CHECKLIST SYNC RUNNER
echo ============================================================
echo  Dynamically executes updates and uploads from Publish-Checklist.md.
echo.
echo  Requirements:
echo  1. Brave must be running with debugging on port 9222.
echo     (Run RUN-Nexus-StartBraveDebug.bat first if needed)
echo  2. You should be logged into Nexus Mods in Brave.
echo.
echo  Usage options:
echo    RUN-Nexus-SyncChecklist.bat --dry-run
echo    RUN-Nexus-SyncChecklist.bat --limit 1
echo    RUN-Nexus-SyncChecklist.bat --only update
echo    RUN-Nexus-SyncChecklist.bat --only upload
echo    RUN-Nexus-SyncChecklist.bat --mod AGF-V3-SERVER-2Weekday
echo ============================================================
echo.

"%PYTHON_EXE%" "%WORKFLOW%\SCRIPT-NexusSyncChecklist.py" %*
set "EXIT_CODE=%ERRORLEVEL%"

echo.
echo Runner finished with exit code %EXIT_CODE%.
pause
exit /b %EXIT_CODE%
