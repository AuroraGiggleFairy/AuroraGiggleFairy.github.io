@echo off
setlocal
cd /d "%~dp0"

set "PYTHON_EXE=python"
if exist "..\.venv\Scripts\python.exe" (
    set "PYTHON_EXE=..\.venv\Scripts\python.exe"
)

echo ============================================================
echo   AGF PUBLISHING -- FULL LIVE BACKGROUND SYNC
echo   Zero tokens. Fully non-interruptive based on Canvas Mode.
echo ============================================================
echo.

echo [1/5] Auditing deltas and syncing Canvas modes...
%PYTHON_EXE% SCRIPT-PublishDelta.py
if errorlevel 1 echo Note: Delta audit warning. Proceeding...

echo.
echo [2/5] Creating unlisted mods on Nexus Mods and 7DaysToDieMods...
call RUN-All-NewMods-BatchCreate.bat %*

echo.
echo [3/5] Pushing updates to existing Nexus Mods listings...
%PYTHON_EXE% NexusMods\Workflow\SCRIPT-NexusUpdate.py --only update

echo.
echo [4/5] Pushing updates to existing 7DaysToDieMods listings...
%PYTHON_EXE% 7DaysToDieMods\Workflow\SCRIPT-7dtdmods-BatchUpdate.py --publish

echo.
echo [5/5] Refreshing Checklist and Publishing Matrix Canvas...
%PYTHON_EXE% SCRIPT-GeneratePublishChecklist.py
%PYTHON_EXE% SCRIPT-SyncPublishingMatrix.py

echo.
echo ============================================================
echo   ALL SYNC OPERATIONS COMPLETE
echo ============================================================
echo.
if not "%~1"=="--no-pause" if not "%~2"=="--no-pause" pause
