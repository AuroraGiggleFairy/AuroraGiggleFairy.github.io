@echo off
setlocal
cd /d "%~dp0"
call "..\RUN-7dtdmods-BatchUpdate.bat" %*
endlocal
