@echo off
setlocal
set "HERE=%~dp0"
cd /d "%HERE%"

if "%~1"=="" (
    echo Drop a Player.log onto this bat, or:
    echo   RUN-LogReader.bat "C:\path\Player.log"
    echo.
    pause
    exit /b 1
)

py -3 "%HERE%SCRIPT-LogReader.py" %*
set "EXIT_CODE=%ERRORLEVEL%"
echo.
if not "%EXIT_CODE%"=="0" (
    echo LogReader failed with code %EXIT_CODE%.
)
pause
exit /b %EXIT_CODE%
