@echo off
setlocal
cd /d "%~dp0"

echo === Start Brave with remote debugging (port 9222) ===
echo Close ALL Brave windows first (check the tray), then press any key.
pause >nul

python SCRIPT-7dtdmods-Playwright.py start-chrome-debug
if errorlevel 1 (
  echo FAILED. Quit Brave completely and try again.
  exit /b 1
)

echo.
echo Brave is ready. Keep it open.
echo Next: RUN-7dtdmods-Create.bat zzzAGF-V3-CLIENT-LawnTractorV3Fix
endlocal
