@echo off
setlocal

set "CANVAS=%USERPROFILE%\.cursor\projects\c-GitHub-7D2D-Mods\canvases\player-reports-checklist.canvas.tsx"

if not exist "%CANVAS%" (
    echo ERROR: Player reports canvas not found:
    echo   %CANVAS%
    echo.
    pause
    exit /b 1
)

echo Opening player reports checklist in Cursor...
cursor "%CANVAS%"
exit /b %ERRORLEVEL%
