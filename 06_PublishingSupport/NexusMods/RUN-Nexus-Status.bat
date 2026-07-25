@echo off
setlocal

set "NEXUS_ROOT=%~dp0"
set "WORKFLOW=%NEXUS_ROOT%Workflow"
set "REPO_ROOT=%NEXUS_ROOT%..\.."
cd /d "%REPO_ROOT%"

set "PYTHON_EXE=%REPO_ROOT%\.venv\Scripts\python.exe"
set "PYTHONDONTWRITEBYTECODE=1"
set "STATUS_MD=%NEXUS_ROOT%Nexus-Status.md"

if not exist "%PYTHON_EXE%" (
    echo ERROR: Python environment not found at "%PYTHON_EXE%"
    echo Run the workspace setup or recreate the .venv before using this launcher.
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
    echo ERROR: No Nexus API key was found in:
    echo   %API_KEY_FILE%
    echo Open that file, paste the key, save it, and run this launcher again.
    echo.
    pause
    exit /b 1
)

echo Comparing every local ReleaseSource mod against Nexus Mods.
echo Writes Nexus-Status.md here, and saves high-confidence Nexus IDs into Workflow config.
echo.

"%PYTHON_EXE%" "%WORKFLOW%\SCRIPT-AuditNexusMods.py"
set "EXIT_CODE=%ERRORLEVEL%"

echo.
if "%EXIT_CODE%"=="0" (
    echo Nexus status report finished.
    echo Wrote: %STATUS_MD%
) else (
    echo Nexus status report could not complete. Exit code: %EXIT_CODE%
)
pause
exit /b %EXIT_CODE%
