# Legacy: Screenshot Saver (v0)

This is the original PowerShell version of SnipKeep. It is kept for reference only. Use the SnipKeep app from the main README instead.

## Install
1. Unzip this folder somewhere permanent, for example `C:\Tools\ScreenshotSaver`.
2. Double-click `Install.cmd`.
3. Pick a folder. It is remembered.

It starts with Windows and adds a tray icon (Open folder, Change folder, Exit).

## Notes
- Saves any image copied to the clipboard, not only snips.
- Checks the clipboard every 500 ms.
- Files are named `Screenshot YYYY-MM-DD HH.mm.ss.png`.
- Folder setting: `%APPDATA%\ScreenshotSaver\folder.txt`.

## Turn off or uninstall
- Stop it now: right-click the tray icon > Exit.
- Stop it at startup: Task Manager > Startup > "Microsoft Windows Based Script Host" > Disable.
- Remove completely: delete `ScreenshotSaver.lnk` from `shell:startup`, then delete this folder.
