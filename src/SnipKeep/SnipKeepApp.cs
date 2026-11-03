using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FormsTimer = System.Windows.Forms.Timer;

namespace SnipKeep
{
    /// <summary>
    /// The running app. Owns the clipboard watcher, the settings and the windows.
    /// Runs with no visible window until the user opens SnipKeep.
    /// </summary>
    internal sealed class SnipKeepApp : ApplicationContext
    {
        // Snip & Sketch can update the clipboard more than once per snip. Wait briefly, then read it once.
        private const int SettleDelayMs = 150;
        private const int MaxClipboardAttempts = 4;

        private readonly SettingsStore _store;
        private readonly Settings _settings;
        private readonly SynchronizationContext _ui;
        private readonly ClipboardWatcher _watcher;
        private readonly ImageWriter _writer = new ImageWriter();
        private readonly Notifier _notifier;
        private readonly FormsTimer _settle;
        private readonly FormsTimer _resume;
        private readonly RegisteredWaitHandle _showWait;
        private readonly RegisteredWaitHandle _quitWait;

        private Task _saveQueue = Task.FromResult(0);
        private int _clipboardAttempts;
        private bool _ready;
        private bool _pausedUntilRestart;
        private Form _window;

        public event EventHandler StateChanged;

        public SnipKeepApp(bool background, WaitHandle showSignal, WaitHandle quitSignal)
        {
            _ui = new WindowsFormsSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(_ui);

            AppIcon = LoadIcon();
            _notifier = new Notifier(AppIcon);
            _store = SettingsStore.ForCurrentUser();
            _ready = _store.Exists;
            _settings = _store.Load();

            // Keep the startup entry pointing at this exe, for example after the portable exe was moved.
            if (_ready) StartupRegistry.Apply(_settings.StartWithWindows, Application.ExecutablePath);

            _settle = new FormsTimer { Interval = SettleDelayMs };
            _settle.Tick += (s, e) => ReadClipboard();

            _resume = new FormsTimer();
            _resume.Tick += (s, e) => CheckPauseExpired();
            UpdateResumeTimer();

            _watcher = new ClipboardWatcher();
            _watcher.ClipboardChanged += (s, e) => OnClipboardChanged();

            _showWait = ThreadPool.RegisterWaitForSingleObject(showSignal, (s, t) => _ui.Post(_ => ShowWindow(), null), null, Timeout.Infinite, false);
            _quitWait = ThreadPool.RegisterWaitForSingleObject(quitSignal, (s, t) => _ui.Post(_ => Quit(), null), null, Timeout.Infinite, true);

            if (!_ready || !background) ShowWindow();
        }

        public Icon AppIcon { get; private set; }

        public Settings Settings
        {
            get { return _settings; }
        }

        public static string Version
        {
            get
            {
                var v = typeof(SnipKeepApp).Assembly.GetName().Version;
                return v.Major + "." + v.Minor + "." + v.Build;
            }
        }

        // ---- Windows ----

        public void ShowWindow()
        {
            if (_window != null && !_window.IsDisposed)
            {
                if (_window.WindowState == FormWindowState.Minimized) _window.WindowState = FormWindowState.Normal;
                _window.Activate();
                return;
            }

            _window = _ready ? (Form)new SettingsForm(this) : new WelcomeForm(this);
            _window.FormClosed += (s, e) => _window = null;
            _window.Show();
            _window.Activate();
        }

        /// <summary>Called by the welcome window. Returns an error message, or null on success.</summary>
        public string CompleteWelcome(string folder)
        {
            try
            {
                Directory.CreateDirectory(folder);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException || ex is NotSupportedException)
            {
                return "SnipKeep could not use this folder. " + ex.Message;
            }

            _settings.Folder = folder;
            if (!SaveSettings()) return "SnipKeep could not save its settings.";

            _ready = true;
            StartupRegistry.Apply(_settings.StartWithWindows, Application.ExecutablePath);
            _notifier.Show("SnipKeep is on", "Press Win+Shift+S to try it.", ToolTipIcon.None, null);
            return null;
        }

        // ---- Settings ----

        public void Change(Action<Settings> change)
        {
            var oldStartup = _settings.StartWithWindows;
            change(_settings);
            SaveSettings();
            if (_settings.StartWithWindows != oldStartup)
            {
                StartupRegistry.Apply(_settings.StartWithWindows, Application.ExecutablePath);
            }
            OnStateChanged();
        }

        private bool SaveSettings()
        {
            try
            {
                _store.Save(_settings);
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                _notifier.Show("SnipKeep could not save its settings", ex.Message, ToolTipIcon.Error, null);
                return false;
            }
        }

        // ---- Pause ----

        public bool IsPaused
        {
            get
            {
                if (_pausedUntilRestart) return true;
                if (!_settings.Paused) return false;
                var until = _settings.PausedUntilTime;
                return !until.HasValue || until.Value > DateTime.Now;
            }
        }

        /// <summary>The time saving resumes, or null when the pause has no end time.</summary>
        public DateTime? PausedUntil
        {
            get { return _pausedUntilRestart ? null : _settings.PausedUntilTime; }
        }

        public bool PausedUntilRestart
        {
            get { return _pausedUntilRestart; }
        }

        public void PauseFor(TimeSpan duration)
        {
            _pausedUntilRestart = false;
            Change(s =>
            {
                s.Paused = true;
                s.PausedUntilTime = DateTime.Now + duration;
            });
            UpdateResumeTimer();
        }

        public void PauseUntilResumed()
        {
            _pausedUntilRestart = false;
            Change(s =>
            {
                s.Paused = true;
                s.PausedUntilTime = null;
            });
            UpdateResumeTimer();
        }

        /// <summary>Pauses until SnipKeep restarts, for example after signing out or restarting Windows.</summary>
        public void PauseUntilRestart()
        {
            _pausedUntilRestart = true;
            Change(s =>
            {
                s.Paused = false;
                s.PausedUntilTime = null;
            });
            UpdateResumeTimer();
        }

        public void Resume()
        {
            _pausedUntilRestart = false;
            Change(s =>
            {
                s.Paused = false;
                s.PausedUntilTime = null;
            });
            UpdateResumeTimer();
        }

        private void CheckPauseExpired()
        {
            if (_settings.Paused && !IsPaused) Resume();
            else UpdateResumeTimer();
        }

        private void UpdateResumeTimer()
        {
            _resume.Stop();
            var until = _settings.Paused ? _settings.PausedUntilTime : null;
            if (!until.HasValue) return;

            var ms = (until.Value - DateTime.Now).TotalMilliseconds;
            _resume.Interval = (int)Math.Max(1000, Math.Min(ms + 500, int.MaxValue));
            _resume.Start();
        }

        // ---- Saving ----

        private void OnClipboardChanged()
        {
            _clipboardAttempts = 0;
            _settle.Stop();
            _settle.Start();
        }

        private void ReadClipboard()
        {
            _settle.Stop();
            if (!_ready || IsPaused) return;

            Image image;
            try
            {
                image = TryGetScreenshot();
            }
            catch (ExternalException)
            {
                // Another app has the clipboard open. Try again shortly.
                if (++_clipboardAttempts < MaxClipboardAttempts) _settle.Start();
                return;
            }
            if (image == null) return;

            var folder = _settings.Folder;
            var pattern = _settings.NamePattern;
            var time = DateTime.Now;

            _saveQueue = _saveQueue.ContinueWith(
                _ =>
                {
                    var result = _writer.Save(image, folder, pattern, time);
                    _ui.Post(r => OnSaved((SaveResult)r), result);
                },
                TaskScheduler.Default);
        }

        private Image TryGetScreenshot()
        {
            var data = Clipboard.GetDataObject();
            if (data == null) return null;

            var formats = data.GetFormats(false);
            if (!ScreenshotFilter.ShouldSave(formats, _settings.ScreenshotsOnly)) return null;

            var image = data.GetDataPresent(DataFormats.Bitmap) ? data.GetData(DataFormats.Bitmap, true) as Image : null;
            if (image != null) return image;

            if (formats.Contains("PNG"))
            {
                var stream = data.GetData("PNG") as Stream;
                if (stream != null)
                {
                    using (stream)
                    using (var decoded = Image.FromStream(stream))
                    {
                        return new Bitmap(decoded);
                    }
                }
            }
            return null;
        }

        private void OnSaved(SaveResult result)
        {
            if (result.Status == SaveStatus.Failed)
            {
                _notifier.Show("SnipKeep could not save", "Click to choose another folder.", ToolTipIcon.Error, ShowWindow);
                return;
            }

            if (result.Status == SaveStatus.Saved && !_settings.FirstSaveShown)
            {
                Change(s => s.FirstSaveShown = true);
                var path = result.Path;
                _notifier.Show("First screenshot saved", "Click to open the folder.", ToolTipIcon.None, () => RevealFile(path));
            }
        }

        // ---- Shell ----

        public void OpenFolder()
        {
            if (Directory.Exists(_settings.Folder)) StartShell("explorer.exe", Quote(_settings.Folder));
        }

        private static void RevealFile(string path)
        {
            if (File.Exists(path)) StartShell("explorer.exe", "/select," + Quote(path));
        }

        public static void OpenUrl(string url)
        {
            StartShell(url, null);
        }

        private static void StartShell(string file, string arguments)
        {
            try
            {
                Process.Start(new ProcessStartInfo(file, arguments ?? "") { UseShellExecute = true });
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // Nothing useful to do if Explorer or the browser cannot start.
            }
        }

        private static string Quote(string path)
        {
            return "\"" + path + "\"";
        }

        // ---- Lifetime ----

        public void Quit()
        {
            ExitThread();
        }

        protected override void ExitThreadCore()
        {
            if (_window != null && !_window.IsDisposed) _window.Close();
            _showWait.Unregister(null);
            _quitWait.Unregister(null);
            _watcher.Dispose();
            _settle.Dispose();
            _resume.Dispose();
            _notifier.Dispose();

            // Let a save that is still running finish.
            _saveQueue.Wait(TimeSpan.FromSeconds(5));
            base.ExitThreadCore();
        }

        private void OnStateChanged()
        {
            var handler = StateChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private static Icon LoadIcon()
        {
            using (var stream = typeof(SnipKeepApp).Assembly.GetManifestResourceStream("SnipKeep.ico"))
            {
                return stream != null ? new Icon(stream) : SystemIcons.Application;
            }
        }
    }
}
