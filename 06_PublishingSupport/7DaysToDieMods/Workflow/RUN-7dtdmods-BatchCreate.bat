@echo off
setlocal
cd /d "%~dp0"
call "..\RUN-7dtdmods-BatchCreate.bat" %*
endlocal
