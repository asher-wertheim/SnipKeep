using System;
using System.Drawing;
using System.Windows.Forms;

namespace SnipKeep
{
    /// <summary>
    /// Shows a Windows notification from a short-lived notification icon.
    /// The icon is removed as soon as the notification closes, so nothing stays in the tray.
    /// </summary>
    internal sealed class Notifier : IDisposable
    {
        private static readonly TimeSpan MaxLifetime = TimeSpan.FromSeconds(20);

        private readonly Icon _icon;
        private NotifyIcon _current;
        private Timer _cleanup;

        public Notifier(Icon icon)
        {
            _icon = icon;
        }

        public void Show(string title, string text, ToolTipIcon kind, Action onClick)
        {
            Close();

            var notifyIcon = new NotifyIcon { Icon = _icon, Text = "SnipKeep", Visible = true };
            notifyIcon.BalloonTipClicked += (s, e) =>
            {
                CloseSoon();
                if (onClick != null) onClick();
            };
            notifyIcon.BalloonTipClosed += (s, e) => CloseSoon();
            notifyIcon.ShowBalloonTip((int)MaxLifetime.TotalMilliseconds, title, text, kind);
            _current = notifyIcon;

            _cleanup = new Timer { Interval = (int)MaxLifetime.TotalMilliseconds };
            _cleanup.Tick += (s, e) => Close();
            _cleanup.Start();
        }

        // The icon raised the event that calls this, so it is disposed a moment later, not inside its own handler.
        private void CloseSoon()
        {
            if (_cleanup == null) return;
            _cleanup.Stop();
            _cleanup.Interval = 100;
            _cleanup.Start();
        }

        private void Close()
        {
            if (_cleanup != null)
            {
                _cleanup.Dispose();
                _cleanup = null;
            }
            if (_current != null)
            {
                _current.Visible = false;
                _current.Dispose();
                _current = null;
            }
        }

        public void Dispose()
        {
            Close();
        }
    }
}
