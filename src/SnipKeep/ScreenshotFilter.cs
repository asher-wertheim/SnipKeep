using System;
using System.Collections.Generic;
using System.Linq;

namespace SnipKeep
{
    /// <summary>Decides from the clipboard formats whether a clipboard update should be saved.</summary>
    internal static class ScreenshotFilter
    {
        // Browsers, Office and File Explorer add these when you copy an image or a file.
        // Snip & Sketch and the Snipping Tool do not.
        private static readonly string[] NotAScreenshot =
        {
            "HTML Format",
            "FileDrop",
            "FileName",
            "FileNameW",
            "Shell IDList Array",
            "Rich Text Format",
            "Art::GVML ClipFormat",
            "Office Drawing Shape Format",
            "Embed Source",
            "Object Descriptor",
            "Link Source",
        };

        // Apps such as password managers set this to ask clipboard tools to skip the content.
        private const string ExcludeFromMonitoring = "ExcludeClipboardContentFromMonitorProcessing";

        private static readonly string[] ImageFormats = { "Bitmap", "DeviceIndependentBitmap", "Format17", "PNG" };

        public static bool ShouldSave(IEnumerable<string> formats, bool screenshotsOnly)
        {
            var set = new HashSet<string>(formats ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);

            if (set.Contains(ExcludeFromMonitoring)) return false;
            if (!ImageFormats.Any(set.Contains)) return false;
            if (screenshotsOnly && NotAScreenshot.Any(set.Contains)) return false;
            return true;
        }
    }
}
