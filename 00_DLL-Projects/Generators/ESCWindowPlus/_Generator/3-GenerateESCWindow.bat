@echo off
setlocal
cd /d "%~dp0.."
set "PYTHONPYCACHEPREFIX=%CD%\_Generator\Code\__pycache__"
echo Running Easy ESC generation...
python _Generator\Code\SCRIPT-GenerateESCMenu.py --easy --config _Generator\Code\ESCMenu.source.json --texts-file _Generator\2-EditESCMenuConfig.txt
echo.
echo Done. Press any key to close.
pause >nul
