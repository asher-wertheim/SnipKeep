using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SnipKeep
{
    /// <summary>
    /// Hidden message-only window that Windows notifies on every clipboard change.
    /// Event-driven, so it uses no CPU while idle.
    /// </summary>
    internal sealed class ClipboardWatcher : NativeWindow, IDisposable
    {
        private const int WM_CLIPBOARDUPDATE = 0x031D;
        private static readonly IntPtr HWND_MESSAGE = new IntPtr(-3);

        public event EventHandler ClipboardChanged;

        public ClipboardWatcher()
        {
            CreateHandle(new CreateParams { Caption = "SnipKeep clipboard watcher", Parent = HWND_MESSAGE });
            if (!AddClipboardFormatListener(Handle))
            {
                var error = Marshal.GetLastWin32Error();
                DestroyHandle();
                throw new System.ComponentModel.Win32Exception(error);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_CLIPBOARDUPDATE)
            {
                var handler = ClipboardChanged;
                if (handler != null) handler(this, EventArgs.Empty);
                m.Result = IntPtr.Zero;
                return;
            }
            base.WndProc(ref m);
        }

        public void Dispose()
        {
            if (Handle != IntPtr.Zero)
            {
                RemoveClipboardFormatListener(Handle);
                DestroyHandle();
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
    }
}
