@echo off
setlocal
cd /d "%~dp006_PublishingSupport"
call RUN-Publish-LiveSync.bat %*
