@echo off
setlocal
cd /d "%~dp0"
echo Creating ESC text template...
python SCRIPT-GenerateESCMenu.py --init-texts-file
echo.
echo Done. Press any key to close.
pause >nul
