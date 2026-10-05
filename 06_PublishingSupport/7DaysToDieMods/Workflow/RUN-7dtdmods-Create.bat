@echo off
setlocal
cd /d "%~dp0"

if "%~1"=="" (
  echo Usage: RUN-7dtdmods-Create.bat AGF-VP-FuelBurnPlus
  echo.
  echo Uses your real Chrome login via CDP port 9222.
  echo If that is not running: RUN-7dtdmods-StartChromeDebug.bat ^(close Chrome first^)
  echo Creates a draft, fills from PublishHelp packet, stops at publish glance.
  echo Does NOT click Publish.
  exit /b 1
)

echo === Prep + create listing: %~1 ===
python SCRIPT-7dtdmods-Playwright.py create "%~1" --no-keep-open
if errorlevel 1 (
  echo FAILED.
  exit /b 1
)
endlocal
