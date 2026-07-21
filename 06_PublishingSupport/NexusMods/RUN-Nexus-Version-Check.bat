@echo off
setlocal

set "REPO_ROOT=%~dp0..\.."
cd /d "%REPO_ROOT%"

set "PYTHON_EXE=%REPO_ROOT%\.venv\Scripts\python.exe"

if not exist "%PYTHON_EXE%" (
    echo ERROR: Python environment not found at "%PYTHON_EXE%"
    echo Run the workspace setup or recreate the .venv before using this launcher.
    echo.
    pause
    exit /b 1
)

set "API_KEY_FILE=%~dp0nexus-api-key.private.txt"

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

echo Checking every local release version against Nexus Mods.
echo This is read-only: it will not upload or change any mods.
echo.

"%PYTHON_EXE%" "%~dp0SCRIPT-AuditNexusMods.py"
set "EXIT_CODE=%ERRORLEVEL%"

echo.
if "%EXIT_CODE%"=="0" (
    echo Nexus version check finished.
) else (
    echo Nexus version check could not complete. Exit code: %EXIT_CODE%
)
pause
exit /b %EXIT_CODE%
