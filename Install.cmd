@echo off
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File "%~dp0ScreenshotSaver.ps1" -Install
echo Installed. Pick your folder in the window that opens.
timeout /t 3 >nul
