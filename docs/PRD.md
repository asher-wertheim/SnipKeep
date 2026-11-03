# SnipKeep: PRD and Spec

Nov 3, 2026 · Asher Wertheim · Status: approved, ready to build

## Summary

SnipKeep is a tiny Windows app that saves every Win+Shift+S screenshot to a folder you pick, like macOS does. You set the folder once and forget about it. It installs in under a minute, needs no admin rights, uses no tray icon and sends no data anywhere.

It replaces the current PowerShell script in `pc-screenshot-saver` with a real installed program: its own name, icon, Start menu entry and uninstaller.

## Problem and goals

On Windows 10, Win+Shift+S copies the snip to the clipboard only. To keep it, you open Snip & Sketch, click Save, pick a folder and type a name. That is 4 extra steps for every screenshot. macOS saves to a set folder by default.

The current script fixes this but feels like a hack. It shows as "Microsoft Windows Based Script Host" in Task Manager, its alerts say "Windows PowerShell", and it has no proper install or uninstall.

**Goals**

1. Zero extra steps per screenshot after setup.
2. Feels like a real, trusted Windows app.
3. Small and light enough that you never notice it running.
4. A clean, well-documented repo that works as a portfolio piece.

**Success metrics**

| Metric | Target |
| --- | --- |
| Install to first saved screenshot | under 60 seconds |
| Steps per screenshot after setup | 0 |
| Installer size | under 2 MB |
| Memory while idle | under 30 MB |
| CPU while idle | 0% (event-driven, no polling) |
| Data sent off the PC | none |
| Snips missed in a 100-snip test | 0 |

## Users and user stories

The main user is a Windows 10 or 11 user who takes many screenshots for work: PMs, ops, support, QA, designers. Many came from a Mac and miss its screenshot flow. They are comfortable installing apps but do not want to configure much.

1. As a user, I want every snip saved to my folder automatically, so I never lose a screenshot.
2. As a user, I want to pick the folder once, so I do not think about it again.
3. As a user, I want the app to start with Windows, so it just works after a restart.
4. As a user, I want to open SnipKeep from the Start menu, so I can change settings without hunting for a tray icon.
5. As a user, I want to pause saving for a while, so private images I copy are not stored.
6. As a user, I want only real screenshots saved, not images I copy from a browser.
7. As a user, I want to be told only when something goes wrong, so Windows' own snip alert is the only one I see.
8. As a user, I want to uninstall it from Settings > Apps like any other program.

## Scope

v1 does one job well: catch the snip, save it, stay out of the way.

**v1 features**

| Feature | Detail |
| --- | --- |
| Auto-save | Every new clipboard screenshot saved as PNG to the chosen folder |
| Screenshots only | On by default. Skips images copied from browsers, Office and file copies |
| Duplicate guard | Skips an image identical to the last one saved |
| Settings window | Folder, file name pattern, start with Windows, screenshots only, pause |
| Open from Start menu | Launching SnipKeep while it runs opens the settings window |
| Pause / resume | One button in settings. Optional auto-resume after 15 min, 1 h or until restart, or stay paused until resumed |
| Start with Windows | On by default. Shows as "SnipKeep" in Task Manager > Startup |
| Error alerts only | Branded "SnipKeep" toast when a save fails, nothing on success |
| Installer | Per-user, no admin, Start menu entry, uninstall from Settings > Apps |
| Portable exe | Same app without installer, for people who prefer it |

**Later (v1.x)**

- Sub-folders by month (`2026-11\`).
- JPG option to save space.
- Open the last screenshot with a hotkey.
- winget and Microsoft Store listing.
- Code signing to remove the SmartScreen warning.

**Not doing**

- Editing, markup or annotation. Snip & Sketch already does this.
- Cloud upload or sharing.
- Our own capture tool or hotkey. We use the Windows one.
- Screenshot history or gallery UI. File Explorer does this.
- macOS or Linux versions.

## UX flows

The user sees SnipKeep twice: at setup and when they want to change something. The rest of the time it is invisible.

**Install and first run**

1. Download `SnipKeep-Setup.exe` from GitHub Releases.
2. Run it. One screen: "Install SnipKeep" with an Install button. No admin prompt.
3. On finish, SnipKeep opens the welcome window: "Where should screenshots go?" with the folder pre-filled as `Pictures\Screenshots`.
4. User clicks Change or keeps the default, then Done.
5. The window closes. A one-time toast says "SnipKeep is on. Press Win+Shift+S to try it."
6. On the first saved screenshot only, a toast says "First screenshot saved. Click to open the folder." This shows the user where files go. Later saves stay silent.

**Daily use**

1. User presses Win+Shift+S and selects an area.
2. Windows shows its own "Snip saved to clipboard" alert.
3. SnipKeep saves the PNG in under 1 second. No alert from SnipKeep.

**Change settings**

1. User opens SnipKeep from the Start menu.
2. The running copy shows its settings window. No second copy starts.
3. Changes save right away. Closing the window keeps SnipKeep running.

**Settings window**

- Status line: "Saving screenshots to D:\Shots" with a Pause button.
- Save to: folder path, Change, Open folder.
- File name: pattern with live preview, for example `Screenshot 2026-11-03 12.30.49.png`.
- Screenshots only: on / off.
- Start with Windows: on / off.
- Footer: version, "Quit SnipKeep" link, GitHub link.

**Failure**

- Folder missing or not writable: toast "SnipKeep could not save. Click to choose another folder." Click opens settings.

**Uninstall**

1. Settings > Apps > SnipKeep > Uninstall.
2. Removes the app, Start menu entry and startup entry. Asks whether to keep settings. Never deletes screenshots.

## Spec sheet

SnipKeep is a C# WinForms app on .NET Framework 4.8, which ships inside Windows 10 (1903+) and 11. That keeps the exe under 300 KB with nothing extra to install.

| Item | Choice | Why |
| --- | --- | --- |
| Language / UI | C#, WinForms | Native look, small, no runtime download |
| Runtime | .NET Framework 4.8 | Built into Windows 10 1903+ and 11 |
| Clipboard watch | `AddClipboardFormatListener` on a hidden window | Event-driven, 0% CPU when idle. Replaces the 500 ms polling in the script |
| Single instance | Named mutex + named event | Second launch tells the first to show settings, then exits |
| Settings | `%APPDATA%\SnipKeep\settings.json` | Easy to read, back up and delete |
| Start with Windows | `HKCU\...\CurrentVersion\Run` value "SnipKeep" | Shows as SnipKeep in Task Manager. No admin |
| Alerts | Short-lived notification icon, errors only | Toast header shows "SnipKeep" with our icon. Nothing stays in the tray |
| Installer | Inno Setup, per-user, `%LOCALAPPDATA%\Programs\SnipKeep` | No admin prompt. Registers in Settings > Apps. Asks a running copy to quit (`--quit`) before update and uninstall |
| Build and release | GitHub Actions on `windows-latest`, triggered by a version tag | Every release is built the same way, from source |
| License | All rights reserved, with an "as is" no-warranty clause | Keeps every option open, including selling later. Can move to open source later |

![SnipKeep architecture](architecture.png)

The top row runs on every clipboard change. The bottom row runs only when the user opens SnipKeep.

**Screenshots-only rule**

A clipboard update counts as a screenshot when it holds a bitmap and none of these formats: `HTML Format`, `FileDrop`, `Rich Text Format`, or Office drawing formats. Content marked `ExcludeClipboardContentFromMonitorProcessing` (used by password managers) is never saved, even with the setting off. Browsers and Office add those when you copy an image. Snip & Sketch does not. This rule must be checked on real Windows 10 and 11 machines before release.

**Settings file**

| Key | Default |
| --- | --- |
| `folder` | `%USERPROFILE%\Pictures\Screenshots` |
| `namePattern` | `Screenshot {yyyy-MM-dd HH.mm.ss}` |
| `screenshotsOnly` | true |
| `startWithWindows` | true |
| `paused` / `pausedUntil` | false / empty |
| `firstSaveShown` | false. Set after the one-time "First screenshot saved" toast |

Name clashes in the same second get ` (2)`, ` (3)` and so on. Characters Windows does not allow are removed from the pattern.

**Performance budgets**

| Measure | Budget |
| --- | --- |
| Snip to file on disk | under 1 s for a 4K screenshot |
| Memory, idle | under 30 MB |
| CPU, idle | 0% |
| App startup | under 0.5 s |
| Exe / installer size | under 300 KB / under 2 MB |

## Release, testing and risks

v1.0 ships as a GitHub Release with two files: `SnipKeep-Setup.exe` and `SnipKeep-portable.zip`. The repo gets a README with a demo GIF, this PRD under `docs/`, and the old script moved to a `legacy/` folder.

**Release steps**

1. Build the app and installer in GitHub Actions.
2. Asher runs the test checklist on his Windows 10 PC.
3. Fix, tag `v1.0.0`, publish the release.
4. Add SnipKeep to the portfolio site and pin the repo.

**Test checklist**

- [ ] Install with no admin prompt
- [ ] First run asks for the folder, default is Pictures\Screenshots
- [ ] 20 snips in a row: 20 files, none missed, no duplicates
- [ ] Image copied from Chrome: not saved when Screenshots only is on
- [ ] Image copied from Word: not saved when Screenshots only is on
- [ ] Pause, snip, resume: snip during pause not saved
- [ ] Restart Windows: SnipKeep running, shows as "SnipKeep" in Task Manager > Startup
- [ ] Open from Start menu while running: settings window, no second copy
- [ ] Folder deleted while running: error toast, click opens settings
- [ ] Uninstall: app, Start menu and startup entry gone, screenshots kept
- [ ] Idle memory under 30 MB after 1 hour

**Risks**

| Risk | Impact | Plan |
| --- | --- | --- |
| Unsigned exe shows "Windows protected your PC" | Users may not trust it | README explains "More info > Run anyway". Consider paid code signing later (free SignPath signing needs an open source license) |
| Antivirus flags an unknown app that reads the clipboard | Install blocked | Public source code, built by GitHub Actions. Submit to Microsoft for review if flagged |
| Screenshots-only rule misses or blocks some images | Wrong files saved or skipped | Test on Windows 10 and 11. Setting can be turned off |
| Windows 11 Snipping Tool already auto-saves | Less need on 11 | Position as Windows 10 first. On 11 it still adds folder choice and naming |
| Claude cannot run Windows builds directly | Bugs found late | Build in GitHub Actions. Asher tests each build |

**Decisions (Nov 3, 2026)**

- Name: SnipKeep. The only exact match found is a small, inactive code-snippet repo. A paid macOS app called SnapKeep exists, so always say "for Windows" in the README and store text.
- Repo renamed from `pc-screenshot-saver` to `snipkeep`.
- License: all rights reserved, with an "as is" no-warranty clause. Replaces MIT so a paid version stays possible.
- Default folder: `Pictures\Screenshots`, plus a one-time toast on the first save. Desktop was rejected because OneDrive often syncs it and it fills up fast.

---

# SnipKeep v2: draft spec

Nov 3, 2026 · Asher Wertheim · Status: draft, decisions from the v2 Q&A. Mockups come before any code.

## Why v2

v1 works but looks bare-bones. v2 keeps the same job (catch the snip, save it, stay out of the way) and makes the whole experience friendly, modern, fast and fun. UX is the top priority.

## Audience and release

- For both a portfolio and real public users.
- v1.0 is released first. v2 work starts after that.
- English only.
- Windows 10 and 11. Windows 10 support is required.

## Look and feel

| Item | Decision |
| --- | --- |
| UI technology | WPF on .NET Framework 4.8. Built into Windows 10 and 11, nothing to download. May move to a newer stack later if it stays Windows 10 compatible |
| Style references | Duolingo, Focus Friend, Bluey's Quest for the Gold Pen: bright, warm, playful, with clear feedback |
| Logo | Design A (crop corners around a down arrow) in the green of design C (`#12B886`) |
| Mascot | Open. Show 3 mascot ideas in the mockups |
| Theme | User picks: System (default), Light, Dark |
| Motion | Playful: bouncy buttons, a small celebration on the first save, a lively joke note |
| Feedback | Every click and change shows a reaction right away (hover, press, a "Saved" tick) |
| Tone | Quirky everywhere, errors too. Every error still says what went wrong and what to do |

## Branding and license

- License: all rights reserved, with an "as is" no-warranty clause (see `LICENSE`).
- Footer, like the Kupa example:
  - Line 1: `SnipKeep • version 2.0.0`
  - Line 2: `© 2026 Asher Wertheim. All rights reserved.`
- The footer shows in the settings window, on every installer screen and in the README.

## Installer

GitHub only, our own installer. Per-user, no admin. Three screens:

1. **Welcome and privacy.** What SnipKeep does, and the promise: no data leaves your PC.
2. **Options.**
   - Start with Windows: on by default.
   - Desktop icon: off by default.
   - Tray icon: off by default.
3. **Folder and finish.** Pick the folder (default `Pictures\Screenshots`), then Finish opens SnipKeep.

Installer size budget: up to 5 MB. The SmartScreen warning stays for now (see Open questions).

## What gets saved

v2 saves only real screenshots. It checks which program put the image on the clipboard, not only the clipboard formats. A setting with three choices replaces the v1 "Screenshots only" checkbox:

- Windows snip tool and Print Screen (default)
- Windows snip tool only
- Print Screen only

If Windows uses the Print Screen key to open the snip tool (the default on Windows 11), Print Screen snips count as snip tool snips. The setting explains this. Content marked `ExcludeClipboardContentFromMonitorProcessing` is never saved.

## Settings window

Same settings as v1, plus:

- Capture source (the three choices above).
- Theme: System, Light, Dark.
- Tray icon: on / off.
- Saved pop-up: a small "Saved" toast after each snip. Off by default.
- Sound on save: off by default.
- Sub-folders by month (`2026-11\`): off by default.
- File type: PNG (default) or JPG.
- Hotkey to open the latest screenshot.
- Dad jokes: on / off.
- App PIN (see Privacy).

## Tray icon

Optional and off by default. Menu:

- Pause / Resume, with 15 minutes, 1 hour and until restart.
- Open folder
- Change folder
- Settings
- Quit

## Dad jokes

- A bank of 1000+ original jokes written for SnipKeep. No copied joke lists.
- The jokes are self-aware: they know an AI wrote them and that they are not great. Each joke can carry a small comment in a smaller font, for example "I'm an AI. I have no excuse for this one."
- Shown on a small sticky note, tilted about 8 degrees, in the bottom right corner of the settings window.
- Each time the window opens, a random joke the user has not seen yet.
- Click the note for the next joke.
- Seen jokes are stored in `%APPDATA%\SnipKeep\jokes-seen.json`. No joke repeats until all are seen. Then a funny "you have read them all" message shows, and the list starts over in a new random order.
- A build-time check rejects jokes that are too similar to each other.
- Jokes can be turned off in settings.

## Privacy

- No network calls. No telemetry. No update check: updates are manual from GitHub.
- **Cloud guard:** warn when the chosen folder is synced by OneDrive or Google Drive, because screenshots would leave the PC.
- **App PIN:** an optional PIN or Windows Hello prompt before SnipKeep opens the folder or settings. The files are not encrypted, so File Explorer can still open them. The setting says so.

## Performance

- Fast and responsive. No UI action waits on disk or the clipboard.
- Window opens in under 0.3 s on a normal PC.
- Idle CPU stays at 0% and idle memory under 50 MB (v1: 30 MB; WPF needs more).

## Process

1. Release v1.0.
2. Mockups of every screen: installer (3 screens), settings window (light and dark), tray menu, joke note, toasts, mascot options.
3. Asher approves the mockups.
4. Build in small steps, each tested by Asher on Windows 10.

## Open questions

- Use ™ after SnipKeep in the footer, or not.
- SmartScreen: live with the warning, pay for code signing (about $10 a month), or add a Microsoft Store version later.
- The default hotkey for "open latest screenshot".
- What a left-click on the tray icon does.
- Does the app PIN also lock the settings window? How to reset a forgotten PIN?
- Mascot: yes or no, and which one.
