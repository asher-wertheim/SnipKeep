# ScreenshotSaver - auto-saves Win+Shift+S snips to a folder (Windows 10)
param([switch]$Install, [switch]$ChangeFolder)

Add-Type -AssemblyName System.Windows.Forms, System.Drawing
Add-Type -Namespace W -Name C -MemberDefinition '[DllImport("user32.dll")] public static extern uint GetClipboardSequenceNumber();'

$cfgDir = Join-Path $env:APPDATA 'ScreenshotSaver'
$cfg    = Join-Path $cfgDir 'folder.txt'
New-Item -ItemType Directory -Force -Path $cfgDir | Out-Null

function Pick-Folder {
    $d = New-Object System.Windows.Forms.FolderBrowserDialog
    $d.Description = 'Choose where to save screenshots'
    if ($d.ShowDialog() -eq 'OK') {
        Set-Content -Path $cfg -Value $d.SelectedPath -Encoding UTF8
        return $d.SelectedPath
    }
    return $null
}

# --- Install: add to Startup and launch ---
if ($Install) {
    Get-ChildItem $PSScriptRoot | Unblock-File -ErrorAction SilentlyContinue
    $vbs = Join-Path $PSScriptRoot 'ScreenshotSaver.vbs'
    $lnk = Join-Path ([Environment]::GetFolderPath('Startup')) 'ScreenshotSaver.lnk'
    $s = (New-Object -ComObject WScript.Shell).CreateShortcut($lnk)
    $s.TargetPath = 'wscript.exe'
    $s.Arguments  = '"' + $vbs + '"'
    $s.IconLocation = (Join-Path $PSScriptRoot 'icon.ico')
    $s.Description  = 'Screenshot Saver'
    $s.Save()
    Start-Process wscript.exe -ArgumentList ('"' + $vbs + '"')
    exit
}

# --- Single instance ---
$created = $false
$mutex = New-Object System.Threading.Mutex($true, 'Global\ScreenshotSaver', [ref]$created)
if (-not $created) { exit }

# --- Folder (asked once, then remembered) ---
$script:folder = $null
if ((Test-Path $cfg) -and -not $ChangeFolder) { $script:folder = (Get-Content $cfg -Raw).Trim() }
if (-not $script:folder -or -not (Test-Path $script:folder)) { $script:folder = Pick-Folder }
if (-not $script:folder) { exit }

# --- Tray icon ---
$tray = New-Object System.Windows.Forms.NotifyIcon
$iconPath = Join-Path $PSScriptRoot 'icon.ico'
$tray.Icon = if (Test-Path $iconPath) { New-Object System.Drawing.Icon($iconPath) } else { [System.Drawing.SystemIcons]::Application }
$tray.Text = 'Screenshot Saver'
$tray.Visible = $true

$menu = New-Object System.Windows.Forms.ContextMenuStrip
$null = $menu.Items.Add('Open folder', $null, { Start-Process explorer.exe $script:folder })
$null = $menu.Items.Add('Change folder', $null, { $f = Pick-Folder; if ($f) { $script:folder = $f } })
$null = $menu.Items.Add('Exit', $null, { $tray.Visible = $false; [System.Windows.Forms.Application]::Exit() })
$tray.ContextMenuStrip = $menu

# --- Watch clipboard ---
$script:seq = [W.C]::GetClipboardSequenceNumber()   # ignore whatever is already on the clipboard
$timer = New-Object System.Windows.Forms.Timer
$timer.Interval = 500
$timer.Add_Tick({
    $s = [W.C]::GetClipboardSequenceNumber()
    if ($s -eq $script:seq) { return }
    try {
        if ([System.Windows.Forms.Clipboard]::ContainsImage()) {
            $img = [System.Windows.Forms.Clipboard]::GetImage()
            if ($img) {
                $base = 'Screenshot ' + (Get-Date -Format 'yyyy-MM-dd HH.mm.ss')
                $path = Join-Path $script:folder "$base.png"
                $i = 2
                while (Test-Path $path) { $path = Join-Path $script:folder "$base ($i).png"; $i++ }
                $img.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
                $img.Dispose()
                $tray.ShowBalloonTip(1500, 'Screenshot saved', (Split-Path $path -Leaf), 'None')
            }
        }
        $script:seq = $s
    } catch {
        # clipboard busy, try again next tick
    }
})
$timer.Start()

[System.Windows.Forms.Application]::Run()
$timer.Stop(); $tray.Dispose(); $mutex.ReleaseMutex()
