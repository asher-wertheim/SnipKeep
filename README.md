# Screenshot Saver

Auto-saves Win+Shift+S screenshots to a folder of your choice on Windows, like macOS does.

## Install
1. Download this repo (Code > Download ZIP) and unzip it somewhere permanent, for example `C:\Tools\ScreenshotSaver`.
2. Double-click `Install.cmd`.
3. Pick a folder. It is remembered.

It starts with Windows and adds a tray icon (Open folder, Change folder, Exit).

## Compatibility
| Windows | Works? |
|---|---|
| 10 | Yes |
| 11 | Yes. Note that the Windows 11 Snipping Tool can already auto-save snips on its own. |
| 7 / 8 | Probably, with Print Screen (Win+Shift+S does not exist there). Not tested. |
| XP | No |

## Notes
- Saves any image copied to the clipboard, not only snips.
- Files are named `Screenshot YYYY-MM-DD HH.mm.ss.png`.
- Folder setting: `%APPDATA%\ScreenshotSaver\folder.txt`.

## Turn off or uninstall
- Stop it now: right-click the tray icon > Exit.
- Stop it at startup: Task Manager > Startup > "Microsoft Windows Based Script Host" > Disable.
- Remove completely: delete `ScreenshotSaver.lnk` from `shell:startup`, then delete this folder.
