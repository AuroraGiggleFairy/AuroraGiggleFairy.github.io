@echo off
setlocal

set "REPORTS=%~dp0Player-Reports.md"

if not exist "%REPORTS%" (
    echo ERROR: Player reports table not found:
    echo   %REPORTS%
    echo.
    pause
    exit /b 1
)

echo Opening Player-Reports.md in Cursor...
cursor "%REPORTS%"
exit /b %ERRORLEVEL%
