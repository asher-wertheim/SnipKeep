using System;
using Microsoft.Win32;

namespace SnipKeep
{
    /// <summary>Adds or removes the "SnipKeep" value under HKCU\...\Run. No admin rights needed.</summary>
    internal static class StartupRegistry
    {
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "SnipKeep";

        public static void Apply(bool enabled, string exePath)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(RunKey))
                {
                    if (key == null) return;
                    if (enabled) key.SetValue(ValueName, "\"" + exePath + "\" " + Program.BackgroundArg);
                    else key.DeleteValue(ValueName, false);
                }
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is System.Security.SecurityException)
            {
                // Policy can block this key. SnipKeep still works, it just will not start with Windows.
            }
        }
    }
}
