@echo off
setlocal
cd /d "%~dp0"

set "PYTHON_EXE=python"
if exist "..\.venv\Scripts\python.exe" (
    set "PYTHON_EXE=..\.venv\Scripts\python.exe"
)

if "%~1"=="--dry-run" goto DO_DRY_RUN
if "%~1"=="--update" goto DO_DELTA_UPDATE
if "%~1"=="--create" goto DO_BATCH_CREATE
if "%~1"=="--all" goto DO_FULL_SYNC

:MENU
cls
echo ============================================================
echo   AGF MULTI-PLATFORM PUBLISHING RUNNER
echo   Token-free automation for 7daystodiemods.com and Nexus Mods
echo ============================================================
echo.
echo   [1] Full Background Sync (Create new + Update existing based on Canvas) [RECOMMENDED]
echo   [2] Fast Delta Update Only (Update existing live mods)
echo   [3] Batch Create Only (Upload unlisted mods to both sites)
echo   [4] Dry Run / Audit Only (Verify deltas and refresh Canvas)
echo   [5] Refresh Publishing Matrix Canvas and Checklist Only
echo   [Q] Quit
echo.
set /p CHOICE="Select an option [1-5, Q]: "

if "%CHOICE%"=="1" goto DO_FULL_SYNC
if "%CHOICE%"=="2" goto DO_DELTA_UPDATE
if "%CHOICE%"=="3" goto DO_BATCH_CREATE
if "%CHOICE%"=="4" goto DO_DRY_RUN
if "%CHOICE%"=="5" goto DO_REFRESH_ONLY
if /I "%CHOICE%"=="Q" exit /b 0
goto MENU

:DO_DELTA_UPDATE
echo.
echo ============================================================
echo   PHASE 1: AUDIT DELTAS
echo ============================================================
%PYTHON_EXE% SCRIPT-PublishDelta.py
if errorlevel 1 (
    echo Delta audit encountered an issue. Proceeding...
)

echo.
echo ============================================================
echo   PHASE 2: NEXUS MODS FAST LIVE UPDATE
echo ============================================================
%PYTHON_EXE% NexusMods\Workflow\SCRIPT-NexusUpdate.py --only update
if errorlevel 1 (
    echo Nexus update finished with warnings or errors.
)

echo.
echo ============================================================
echo   PHASE 3: 7DAYSTODIEMODS FAST DELTA UPDATE
echo ============================================================
%PYTHON_EXE% 7DaysToDieMods\Workflow\SCRIPT-7dtdmods-BatchUpdate.py --publish
if errorlevel 1 (
    echo 7daystodiemods update finished with warnings or errors.
)

echo.
echo ============================================================
echo   PHASE 4: SYNC PUBLISHING MATRIX CANVAS
echo ============================================================
%PYTHON_EXE% SCRIPT-SyncPublishingMatrix.py
echo.
echo Done! All updates completed.
pause
exit /b 0

:DO_BATCH_CREATE
echo.
echo ============================================================
echo   BATCH CREATE UNLISTED MODS
echo ============================================================
call RUN-All-NewMods-BatchCreate.bat
%PYTHON_EXE% SCRIPT-SyncPublishingMatrix.py
echo.
echo Batch creation pass complete.
pause
exit /b 0

:DO_FULL_SYNC
echo.
echo ============================================================
echo   FULL SYNC: BATCH CREATE + FAST DELTA UPDATE
echo ============================================================
%PYTHON_EXE% SCRIPT-PublishDelta.py
call RUN-All-NewMods-BatchCreate.bat
%PYTHON_EXE% NexusMods\Workflow\SCRIPT-NexusUpdate.py --only update
%PYTHON_EXE% 7DaysToDieMods\Workflow\SCRIPT-7dtdmods-BatchUpdate.py --publish
%PYTHON_EXE% SCRIPT-GeneratePublishChecklist.py
%PYTHON_EXE% SCRIPT-SyncPublishingMatrix.py
echo.
echo Full sync pass complete.
pause
exit /b 0

:DO_DRY_RUN
echo.
echo ============================================================
echo   DRY RUN AUDIT
echo ============================================================
%PYTHON_EXE% SCRIPT-PublishDelta.py --dry-run
echo.
echo --- Nexus Mods Update Dry Run ---
%PYTHON_EXE% NexusMods\Workflow\SCRIPT-NexusUpdate.py --only update --dry-run
echo.
echo --- 7DaysToDieMods Update Dry Run ---
%PYTHON_EXE% 7DaysToDieMods\Workflow\SCRIPT-7dtdmods-BatchUpdate.py --dry-run
echo.
%PYTHON_EXE% SCRIPT-SyncPublishingMatrix.py
echo.
echo Dry run complete.
pause
exit /b 0

:DO_REFRESH_ONLY
echo.
echo ============================================================
echo   REFRESH CHECKLIST AND PUBLISHING MATRIX CANVAS
echo ============================================================
%PYTHON_EXE% SCRIPT-GeneratePublishChecklist.py
%PYTHON_EXE% SCRIPT-SyncPublishingMatrix.py
echo.
echo Publishing Matrix Canvas and Checklist refreshed.
pause
exit /b 0
