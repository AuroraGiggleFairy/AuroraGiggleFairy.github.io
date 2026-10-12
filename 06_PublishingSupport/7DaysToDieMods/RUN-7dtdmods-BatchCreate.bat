@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
set "WORKFLOW_DIR=%SCRIPT_DIR%Workflow"
set "REPO_ROOT=%SCRIPT_DIR%..\.."
cd /d "%WORKFLOW_DIR%"

set "PYTHON_EXE=python"
if exist "%REPO_ROOT%\.venv\Scripts\python.exe" (
    set "PYTHON_EXE=%REPO_ROOT%\.venv\Scripts\python.exe"
)

echo ============================================================
echo   7DAYSTODIEMODS BATCH CREATE
echo   Creates listings for all new mods on 7daystodiemods.com
echo ============================================================
echo.

%PYTHON_EXE% SCRIPT-7dtdmods-BatchCreate.py %*
if errorlevel 1 (
    echo.
    echo Batch create finished with errors.
    pause
    exit /b 1
)
endlocal
