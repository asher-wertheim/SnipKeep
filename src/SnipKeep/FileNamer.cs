using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SnipKeep
{
    /// <summary>
    /// Turns a name pattern like "Screenshot {yyyy-MM-dd HH.mm.ss}" into a file name.
    /// Text inside braces is a .NET date and time format.
    /// </summary>
    internal static class FileNamer
    {
        private const string Fallback = "Screenshot";
        private static readonly Regex Token = new Regex(@"\{([^{}]*)\}", RegexOptions.Compiled);
        private static readonly char[] Invalid = Path.GetInvalidFileNameChars();

        /// <summary>Returns the file name without extension, or null when the pattern is not valid.</summary>
        public static string Format(string pattern, DateTime time)
        {
            if (pattern == null) return null;

            string name;
            try
            {
                name = Token.Replace(pattern, m => time.ToString(m.Groups[1].Value, CultureInfo.InvariantCulture));
            }
            catch (FormatException)
            {
                return null;
            }

            if (name.IndexOf('{') >= 0 || name.IndexOf('}') >= 0) return null;

            var clean = new StringBuilder(name.Length);
            foreach (var c in name.Where(c => Array.IndexOf(Invalid, c) < 0)) clean.Append(c);

            // Windows does not allow names that end with a space or a dot.
            var result = clean.ToString().TrimStart().TrimEnd(' ', '.');
            return result.Length == 0 ? Fallback : result;
        }

        /// <summary>Like Format, but falls back to the default pattern when the pattern is not valid.</summary>
        public static string FormatOrDefault(string pattern, DateTime time)
        {
            return Format(pattern, time) ?? Format(Settings.DefaultNamePattern, time);
        }

        /// <summary>Returns a path in the folder that does not exist yet: "name.png", "name (2).png" and so on.</summary>
        public static string UniquePath(string folder, string baseName, Func<string, bool> exists)
        {
            var path = Path.Combine(folder, baseName + ".png");
            for (var i = 2; exists(path); i++)
            {
                path = Path.Combine(folder, baseName + " (" + i.ToString(CultureInfo.InvariantCulture) + ").png");
            }
            return path;
        }
    }
}
