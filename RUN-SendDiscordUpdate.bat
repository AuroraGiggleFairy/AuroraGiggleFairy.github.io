@echo off
setlocal

cd /d "%~dp0"

set "PYTHON_EXE=%~dp0.venv\Scripts\python.exe"
set "SCRIPT=%~dp005_ReleaseData\GigglePack\Discord\SCRIPT-SendDiscordUpdate.py"

if not exist "%PYTHON_EXE%" (
    echo ERROR: Python environment not found at "%PYTHON_EXE%"
    echo Run the workspace setup or recreate the .venv before using this launcher.
    echo.
    pause
    exit /b 1
)

if not exist "%SCRIPT%" (
    echo ERROR: Discord send script not found at "%SCRIPT%"
    echo.
    pause
    exit /b 1
)

echo ======================================================================
echo Send GigglePack Discord release message
echo ======================================================================
echo.
echo Posts discord-post.txt via webhook.
echo Set AGF_DISCORD_WEBHOOK_URL or pass --discord-webhook-url.
echo Tip: add --dry-run to preview without sending.
echo.
echo ======================================================================
echo.
"%PYTHON_EXE%" "%SCRIPT%" %*
set "EXIT_CODE=%ERRORLEVEL%"

echo.
if %EXIT_CODE% EQU 0 (
    echo Discord send completed successfully.
) else (
    echo Discord send exited with code %EXIT_CODE%.
)
echo.
pause
exit /b %EXIT_CODE%
