# Screenshot Saver

Auto-saves Win+Shift+S screenshots to a folder of your choice on Windows 10, like macOS does.

## Install
1. Put this folder somewhere permanent, for example `C:\Tools\ScreenshotSaver`.
2. Double-click `Install.cmd`.
3. Pick a folder. It is remembered.

It starts with Windows and adds a tray icon (Open folder, Change folder, Exit).

## Notes
- Saves any image copied to the clipboard, not only snips.
- Files are named `Screenshot YYYY-MM-DD HH.mm.ss.png`.
- Folder setting: `%APPDATA%\ScreenshotSaver\folder.txt`.

## Uninstall
Exit from the tray, then delete `ScreenshotSaver.lnk` from `shell:startup`.
