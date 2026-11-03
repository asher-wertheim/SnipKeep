# SnipKeep for Windows

SnipKeep saves every Win+Shift+S screenshot to a folder you pick, like macOS does. Set the folder once and forget about it.

![SnipKeep demo: a snip is saved to the Screenshots folder](docs/demo.gif)

- Saves each snip as a PNG in under a second. No extra clicks.
- Skips images you copy from a browser, Office or File Explorer.
- Starts with Windows. No tray icon, no admin rights.
- Tells you only when a save fails.
- Sends no data anywhere. No telemetry, no network calls.

## Install

1. Download `SnipKeep-Setup.exe` from the [latest release](https://github.com/asher-wertheim/SnipKeep/releases/latest).
2. Run it and click Install.
3. Pick a folder, or keep the default `Pictures\Screenshots`, and click Done.
4. Press Win+Shift+S and select an area. The file is in your folder.

Prefer no installer? Download `SnipKeep-portable.zip`, unzip it anywhere and run `SnipKeep.exe`.

### "Windows protected your PC"

SnipKeep is not code signed yet, so Windows SmartScreen may warn you the first time. Click **More info**, then **Run anyway**. The app is built from this source code by [GitHub Actions](.github/workflows/build.yml).

## Use

SnipKeep runs in the background. To change settings, open SnipKeep from the Start menu.

| Setting | What it does |
| --- | --- |
| Save to | The folder for your screenshots |
| File name | Name pattern. Date parts go in braces, for example `Screenshot {yyyy-MM-dd HH.mm.ss}` |
| Screenshots only | Skip images copied from browsers, Office and files |
| Start with Windows | Start SnipKeep when you sign in |
| Pause | Stop saving for 15 minutes, 1 hour, until SnipKeep restarts, or until you resume |

Changes save right away. Closing the window keeps SnipKeep running. To stop it, click **Quit SnipKeep** at the bottom of the window.

Settings are stored in `%APPDATA%\SnipKeep\settings.json`.

## Requirements

Windows 10 (version 1903 or later) or Windows 11. SnipKeep uses .NET Framework 4.8, which is part of Windows, so there is nothing else to install.

On Windows 11 the Snipping Tool can save snips on its own. SnipKeep still adds folder choice and file naming.

## Uninstall

Settings > Apps > SnipKeep > Uninstall. This removes the app, the Start menu entry and the startup entry. Your screenshots are never deleted.

## Build from source

You need the [.NET SDK](https://dotnet.microsoft.com/download) 8 or later on Windows, and [Inno Setup 6](https://jrsoftware.org/isinfo.php) for the installer.

```bat
dotnet test tests\SnipKeep.Tests
dotnet build src\SnipKeep -c Release -o out\app
iscc installer\SnipKeep.iss
```

| Path | Contents |
| --- | --- |
| `src/SnipKeep` | The app: C# WinForms on .NET Framework 4.8 |
| `tests/SnipKeep.Tests` | Unit tests for file naming, the screenshot rule and settings |
| `installer` | Inno Setup script for the per-user installer |
| `docs/PRD.md` | Product spec |
| `legacy` | The original PowerShell version |

### How it works

A hidden window registers with `AddClipboardFormatListener`, so Windows tells SnipKeep about each clipboard change. There is no polling, so it uses no CPU while idle. When the clipboard holds a bitmap and none of the formats that browsers and Office add (such as `HTML Format` or `Rich Text Format`), SnipKeep saves it as a PNG. A second launch signals the running copy to show its settings window, then exits.

## License

Copyright (c) 2026 Asher Wertheim. All rights reserved. See [LICENSE](LICENSE).
