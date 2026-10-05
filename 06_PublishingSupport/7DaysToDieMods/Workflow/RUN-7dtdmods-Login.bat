@echo off
setlocal
cd /d "%~dp0"

echo === 7dtdmods Playwright: one-time login ===
echo A Chromium window will open. Log into 7daystodiemods.com, then return here and press Enter.
echo.
python SCRIPT-7dtdmods-Playwright.py login
if errorlevel 1 (
  echo FAILED. If Playwright is missing:
  echo   python -m pip install playwright
  echo   python -m playwright install chromium
  exit /b 1
)
echo.
echo Login profile saved under _playwright_profile\
endlocal
