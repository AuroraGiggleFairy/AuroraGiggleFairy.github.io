@echo off
setlocal
cd /d "%~dp0"

set "PYTHON_EXE=%~dp0..\..\.venv\Scripts\python.exe"
if not exist "%PYTHON_EXE%" set "PYTHON_EXE=python"

echo.
echo  ModSync Generator
echo  Edit ADMIN-CONFIG.txt first, then run this.
echo.

"%PYTHON_EXE%" "%~dp0generate.py"
set "EXIT_CODE=%ERRORLEVEL%"
echo.
if not "%EXIT_CODE%"=="0" (
    echo Generate failed.
) else (
    echo Done. Send the .bat in Output\ to players. Nothing else is needed.
)
pause
exit /b %EXIT_CODE%
