@echo off
setlocal
cd /d "%~dp0"
echo Running Easy ESC generation...
python SCRIPT-GenerateESCMenu.py --easy
echo.
echo Done. Press any key to close.
pause >nul
