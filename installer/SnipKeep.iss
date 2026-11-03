; SnipKeep installer (Inno Setup 6)
; Per-user install to %LOCALAPPDATA%\Programs\SnipKeep. No admin rights needed.
; Build: iscc /DAppVersion=1.0.0 /DSourceDir=..\out\app installer\SnipKeep.iss

#ifndef AppVersion
  #define AppVersion "0.0.0"
#endif
#ifndef SourceDir
  #define SourceDir "..\out\app"
#endif

#define AppName "SnipKeep"
#define AppExe "SnipKeep.exe"
#define AppUrl "https://github.com/asher-wertheim/SnipKeep"

[Setup]
AppId={{6B0E3C8A-5F2D-4D7A-9E41-2C8B7A1D5F30}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher=Asher Wertheim
AppPublisherURL={#AppUrl}
AppSupportURL={#AppUrl}/issues
AppUpdatesURL={#AppUrl}/releases
VersionInfoVersion={#AppVersion}
VersionInfoDescription={#AppName} Setup

PrivilegesRequired=lowest
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
UninstallDisplayName={#AppName}
UninstallDisplayIcon={app}\{#AppExe}

; One screen: "Install SnipKeep" with an Install button.
DisableWelcomePage=yes
DisableDirPage=yes
DisableProgramGroupPage=yes
DisableReadyMemo=yes
DisableFinishedPage=yes

; A running SnipKeep is asked to quit by the [Code] section below, so no restart prompt is needed.
CloseApplications=no

MinVersion=10.0
WizardStyle=modern
SetupIconFile=..\src\SnipKeep\SnipKeep.ico
OutputDir=..\out
OutputBaseFilename=SnipKeep-Setup
Compression=lzma2/max
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Messages]
ReadyLabel1=Install SnipKeep
ReadyLabel2a=SnipKeep saves every Win+Shift+S screenshot to a folder you pick. Click Install to continue.
WizardReady=Install SnipKeep

[Files]
Source: "{#SourceDir}\{#AppExe}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceDir}\{#AppExe}.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\LICENSE"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExe}"; Comment: "Save Win+Shift+S screenshots to a folder"

[Run]
; Opens the welcome window on first install, or the settings window after an update.
Filename: "{app}\{#AppExe}"; Flags: nowait

[Code]
const
  RunKey = 'Software\Microsoft\Windows\CurrentVersion\Run';

{ Asks a running SnipKeep to quit, so its files can be replaced or removed. }
procedure QuitRunningSnipKeep();
var
  Exe: String;
  ResultCode: Integer;
begin
  Exe := ExpandConstant('{app}\{#AppExe}');
  if FileExists(Exe) then
    Exec(Exe, '--quit', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  QuitRunningSnipKeep();
  Result := '';
end;

function InitializeUninstall(): Boolean;
begin
  QuitRunningSnipKeep();
  Result := True;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  SettingsDir: String;
begin
  if CurUninstallStep = usUninstall then
  begin
    { SnipKeep writes this value itself, so remove it here. }
    RegDeleteValue(HKEY_CURRENT_USER, RunKey, 'SnipKeep');

    { Screenshots are never deleted. Settings are kept unless the user says otherwise. }
    SettingsDir := ExpandConstant('{userappdata}\SnipKeep');
    if DirExists(SettingsDir) and not UninstallSilent then
    begin
      if MsgBox('Keep your SnipKeep settings?' + #13#10#13#10 +
                'Your screenshots are not deleted either way.',
                mbConfirmation, MB_YESNO) = IDNO then
        DelTree(SettingsDir, True, True, True);
    end;
  end;
end;
