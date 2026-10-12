@echo off
setlocal
cd /d "%~dp0"
call "..\RUN-Nexus-BatchCreate.bat" %*
endlocal
