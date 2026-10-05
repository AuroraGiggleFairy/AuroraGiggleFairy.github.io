@echo off
setlocal
set PYTHONDONTWRITEBYTECODE=1
set "GH=C:\Program Files\GitHub CLI\gh.exe"
if not exist "%GH%" set "GH=%LOCALAPPDATA%\GitHub CLI\gh.exe"

echo Checking GitHub login...
"%GH%" auth status
if errorlevel 1 (
  echo.
  echo Not logged in. Run this once in a terminal:
  echo   "%GH%" auth login
  echo Then run this bat again.
  exit /b 1
)

echo Creating public release-only repo AuroraGiggleFairy/AGF-BackpackPlus-072Slots ...
"%GH%" repo create AuroraGiggleFairy/AGF-BackpackPlus-072Slots --public --add-readme --description "Release-only repo for 7DaysToDieMods sync. Source stays in the AGF workspace."
if errorlevel 1 (
  echo Repo create failed or it already exists. Continuing to release...
)

echo Creating release 4.1.1 and attaching the zip...
"%GH%" release create 4.1.1 --repo AuroraGiggleFairy/AGF-BackpackPlus-072Slots --title 4.1.1 --notes "Fixes to automation and readme generations." "C:\GitHub\7D2D-Mods\04_DownloadZips\AGF-BackpackPlus-072Slots.zip"
if errorlevel 1 (
  echo Release create failed.
  exit /b 1
)

echo.
echo Done. Repo: https://github.com/AuroraGiggleFairy/AGF-BackpackPlus-072Slots
endlocal
