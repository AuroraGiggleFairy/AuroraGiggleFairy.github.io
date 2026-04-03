@echo off
setlocal
cd /d "%~dp0.."
set "PYTHONPYCACHEPREFIX=%CD%\_Generator\Code\__pycache__"
echo Creating ESC text template...
python _Generator\Code\SCRIPT-GenerateESCMenu.py --init-texts-file --texts-file _Generator\2-EditESCMenuConfig.txt
echo.
echo Done. Press any key to close.
pause >nul
