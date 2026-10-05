@echo off
setlocal
set "PUB=%~dp0"
set "REPO=%PUB%.."
cd /d "%REPO%"

set "PYTHON_EXE=%REPO%\.venv\Scripts\python.exe"
if not exist "%PYTHON_EXE%" set "PYTHON_EXE=python"

echo === Refresh Publish-Checklist.md ===
echo Optional: pass --fetch-7dtdmods to compare live 7dtdmods versions.
echo.

"%PYTHON_EXE%" "%PUB%SCRIPT-RefreshPublishChecklist.py" %*
set "EXIT_CODE=%ERRORLEVEL%"
echo.
if "%EXIT_CODE%"=="0" (
  echo Checklist updated: %PUB%Publish-Checklist.md
) else (
  echo FAILED. Exit %EXIT_CODE%
)
exit /b %EXIT_CODE%
