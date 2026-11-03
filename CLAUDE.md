# CLAUDE.md

Project brief for Claude Code sessions. Read this first.

## What this is
SnipKeep: a small Windows app that auto-saves Win+Shift+S screenshots to a folder the user picks, like macOS. Owner: Asher Wertheim. It is meant as a portfolio project, so code, README and commit history should look clean and professional.

Full product spec: `docs/PRD.md`. Follow it. If a change goes against the PRD, say so and update the PRD in the same change.

## Current state (Nov 3, 2026)
- The repo root still holds the v0 PowerShell version (`ScreenshotSaver.ps1`, `.vbs`, `Install.cmd`, `icon.ico`). It works on Windows 10 and is in daily use.
- v1 (C# app + installer) is specced and approved in `docs/PRD.md`. Not started. Decisions are at the end of the PRD.

## Plan for v1
1. Move the v0 files to `legacy/`.
2. `src/SnipKeep/`: C# WinForms, .NET Framework 4.8, SDK-style csproj. No NuGet runtime dependencies.
3. `installer/SnipKeep.iss`: Inno Setup, per-user install, no admin.
4. `.github/workflows/build.yml`: build on `windows-latest` for every push, publish a GitHub Release on `v*` tags.
5. New README with install steps, a demo GIF and the SmartScreen note.

## Rules
- Claude cannot run or test Windows builds here. Build in GitHub Actions. Asher tests on his Windows 10 PC using the checklist in the PRD.
- Keep it simple. No tray icon. No telemetry or network calls.
- Writing (README, UI text, commits): plain short sentences, no em dashes, no curly quotes, no marketing words.
- Commit as Asher with his GitHub no-reply email: `256417061+asher-wertheim@users.noreply.github.com`. Pushes with his private email are rejected.
