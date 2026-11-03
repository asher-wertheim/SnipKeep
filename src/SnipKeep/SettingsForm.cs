using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace SnipKeep
{
    /// <summary>The settings window. Changes save right away. Closing it keeps SnipKeep running.</summary>
    internal sealed class SettingsForm : Form
    {
        private const string RepoUrl = "https://github.com/asher-wertheim/SnipKeep";

        private readonly SnipKeepApp _app;
        private readonly Label _status;
        private readonly Button _pause;
        private readonly ContextMenuStrip _pauseMenu;
        private readonly TextBox _folder;
        private readonly TextBox _pattern;
        private readonly Label _preview;
        private readonly CheckBox _screenshotsOnly;
        private readonly CheckBox _startWithWindows;
        private bool _loading;

        public SettingsForm(SnipKeepApp app)
        {
            _app = app;
            Ui.SetupForm(this, "SnipKeep", app.AppIcon);

            // Status and pause
            _status = Ui.Label("", bold: true, topMargin: 6);
            _status.AutoSize = false;
            _status.Width = 340;
            _status.Height = 2 * _status.Font.Height;
            _status.AutoEllipsis = true;
            _pause = Ui.Button("Pause");
            _pauseMenu = new ContextMenuStrip();
            _pauseMenu.Items.Add("For 15 minutes", null, (s, e) => _app.PauseFor(TimeSpan.FromMinutes(15)));
            _pauseMenu.Items.Add("For 1 hour", null, (s, e) => _app.PauseFor(TimeSpan.FromHours(1)));
            _pauseMenu.Items.Add("Until SnipKeep restarts", null, (s, e) => _app.PauseUntilRestart());
            _pauseMenu.Items.Add("Until I resume", null, (s, e) => _app.PauseUntilResumed());
            _pause.Click += (s, e) =>
            {
                if (_app.IsPaused) _app.Resume();
                else _pauseMenu.Show(_pause, new Point(0, _pause.Height));
            };

            // Folder
            _folder = Ui.TextBox(340);
            _folder.ReadOnly = true;
            var change = Ui.Button("Change...");
            change.Click += (s, e) =>
            {
                var picked = Ui.PickFolder(this, _app.Settings.Folder);
                if (picked != null) _app.Change(x => x.Folder = picked);
            };
            var open = Ui.Button("Open folder");
            open.Click += (s, e) => _app.OpenFolder();

            // File name
            _pattern = Ui.TextBox(340);
            _pattern.TextChanged += (s, e) => OnPatternChanged();
            var reset = Ui.Button("Reset");
            reset.Click += (s, e) => _pattern.Text = Settings.DefaultNamePattern;
            _preview = Ui.Label("");
            _preview.ForeColor = SystemColors.GrayText;

            // Options
            _screenshotsOnly = new CheckBox
            {
                Text = "Screenshots only (skip images copied from browsers, Office and files)",
                AutoSize = true,
                Margin = new Padding(0, 12, 0, 4),
            };
            _screenshotsOnly.CheckedChanged += (s, e) =>
            {
                if (!_loading) _app.Change(x => x.ScreenshotsOnly = _screenshotsOnly.Checked);
            };
            _startWithWindows = new CheckBox { Text = "Start with Windows", AutoSize = true, Margin = new Padding(0, 0, 0, 4) };
            _startWithWindows.CheckedChanged += (s, e) =>
            {
                if (!_loading) _app.Change(x => x.StartWithWindows = _startWithWindows.Checked);
            };

            // Footer
            var version = Ui.Label("SnipKeep " + SnipKeepApp.Version);
            version.ForeColor = SystemColors.GrayText;
            var github = new LinkLabel { Text = "GitHub", AutoSize = true, Margin = new Padding(16, 0, 0, 0) };
            github.LinkClicked += (s, e) => SnipKeepApp.OpenUrl(RepoUrl);
            var quit = new LinkLabel { Text = "Quit SnipKeep", AutoSize = true, Margin = new Padding(16, 0, 0, 0) };
            quit.LinkClicked += (s, e) => _app.Quit();
            var footer = Ui.Row(version, github, quit);
            footer.Margin = new Padding(0, 16, 0, 0);

            var layout = Ui.Column();
            layout.Controls.Add(Ui.Row(_status, _pause));
            layout.Controls.Add(Ui.Label("Save to", topMargin: 12));
            layout.Controls.Add(Ui.Row(_folder, change, open));
            layout.Controls.Add(Ui.Label("File name", topMargin: 8));
            layout.Controls.Add(Ui.Row(_pattern, reset));
            layout.Controls.Add(_preview);
            layout.Controls.Add(_screenshotsOnly);
            layout.Controls.Add(_startWithWindows);
            layout.Controls.Add(footer);
            Controls.Add(layout);

            _app.StateChanged += OnAppStateChanged;
            LoadValues();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _app.StateChanged -= OnAppStateChanged;
            _pauseMenu.Dispose();
            base.OnFormClosed(e);
        }

        private void OnAppStateChanged(object sender, EventArgs e)
        {
            LoadValues();
        }

        private void LoadValues()
        {
            _loading = true;
            var settings = _app.Settings;

            _status.Text = StatusText();
            _pause.Text = _app.IsPaused ? "Resume" : "Pause";
            _folder.Text = settings.Folder;
            if (!_pattern.Focused && _pattern.Text != settings.NamePattern) _pattern.Text = settings.NamePattern;
            _screenshotsOnly.Checked = settings.ScreenshotsOnly;
            _startWithWindows.Checked = settings.StartWithWindows;
            UpdatePreview();

            _loading = false;
        }

        private string StatusText()
        {
            if (!_app.IsPaused) return "Saving screenshots to " + _app.Settings.Folder;
            if (_app.PausedUntilRestart) return "Paused until SnipKeep restarts";
            var until = _app.PausedUntil;
            if (until.HasValue) return "Paused until " + until.Value.ToString("t", CultureInfo.CurrentCulture);
            return "Paused";
        }

        private void OnPatternChanged()
        {
            UpdatePreview();
            if (_loading) return;
            if (FileNamer.Format(_pattern.Text, DateTime.Now) != null && _pattern.Text != _app.Settings.NamePattern)
            {
                var pattern = _pattern.Text;
                _app.Change(x => x.NamePattern = pattern);
            }
        }

        private void UpdatePreview()
        {
            var name = FileNamer.Format(_pattern.Text, DateTime.Now);
            _preview.Text = name != null
                ? "Example: " + name + ".png"
                : "This pattern is not valid. Put date parts in braces, for example {yyyy-MM-dd}.";
        }
    }
}
