@echo off
setlocal
cd /d "%~dp0"

if "%~1"=="" (
  echo Usage: RUN-7dtdmods-Update.bat AGF-VP-FuelBurnPlus [--zip] [--changelog] [--desc-only] [--images-only]
  echo Needs site_mod_id in 7dtdmods-config.json ^(saved automatically by Create^).
  exit /b 1
)

echo === Update listing: %~1 ===
python SCRIPT-7dtdmods-Playwright.py update %*
if errorlevel 1 (
  echo FAILED.
  exit /b 1
)
endlocal
