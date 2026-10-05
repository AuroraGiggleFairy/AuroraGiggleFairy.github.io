@echo off
setlocal
cd /d "%~dp0"
title Doom Test Server
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Install-DoomTest.ps1"
echo.
pause
exit /b %ERRORLEVEL%
