using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace SnipKeep
{
    internal static class Program
    {
        /// <summary>Passed by the startup entry: start without showing a window.</summary>
        public const string BackgroundArg = "--background";

        /// <summary>Passed by the uninstaller: ask the running copy to quit.</summary>
        public const string QuitArg = "--quit";

        // Only one copy runs per user session. A second launch signals the first one and exits.
        public const string MutexName = "SnipKeep.Running";
        private const string ShowEventName = "SnipKeep.ShowWindow";
        private const string QuitEventName = "SnipKeep.Quit";

        [STAThread]
        private static int Main(string[] args)
        {
            var background = args.Contains(BackgroundArg, StringComparer.OrdinalIgnoreCase);
            var quit = args.Contains(QuitArg, StringComparer.OrdinalIgnoreCase);

            bool firstInstance;
            using (var mutex = new Mutex(true, MutexName, out firstInstance))
            {
                if (!firstInstance)
                {
                    if (quit) QuitRunningCopy(mutex);
                    else Signal(ShowEventName, allowForeground: true);
                    return 0;
                }

                if (quit) return 0;

                using (var showEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ShowEventName))
                using (var quitEvent = new EventWaitHandle(false, EventResetMode.AutoReset, QuitEventName))
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new SnipKeepApp(background, showEvent, quitEvent));
                }

                mutex.ReleaseMutex();
            }
            return 0;
        }

        private static void Signal(string name, bool allowForeground)
        {
            EventWaitHandle handle;
            if (!EventWaitHandle.TryOpenExisting(name, out handle)) return;
            using (handle)
            {
                // Let the running copy bring its window to the front.
                if (allowForeground) AllowSetForegroundWindow(ASFW_ANY);
                handle.Set();
            }
        }

        private static void QuitRunningCopy(Mutex mutex)
        {
            Signal(QuitEventName, allowForeground: false);
            try
            {
                // The running copy owns the mutex. Getting it means that copy has exited.
                if (mutex.WaitOne(TimeSpan.FromSeconds(10))) mutex.ReleaseMutex();
            }
            catch (AbandonedMutexException)
            {
                // The other copy exited without releasing the mutex. That is fine here.
            }
        }

        private const int ASFW_ANY = -1;

        [DllImport("user32.dll")]
        private static extern bool AllowSetForegroundWindow(int processId);
    }
}
