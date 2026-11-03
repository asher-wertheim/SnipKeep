using Xunit;

namespace SnipKeep.Tests
{
    public class ScreenshotFilterTests
    {
        private static readonly string[] Snip = { "Bitmap", "DeviceIndependentBitmap", "Format17", "PNG" };
        private static readonly string[] Browser = { "HTML Format", "Bitmap", "DeviceIndependentBitmap", "Format17" };
        private static readonly string[] Word = { "Art::GVML ClipFormat", "Office Drawing Shape Format", "Bitmap", "PNG", "Object Descriptor" };
        private static readonly string[] Explorer = { "Shell IDList Array", "FileDrop", "FileNameW", "Bitmap" };
        private static readonly string[] Text = { "UnicodeText", "Text", "Locale" };

        [Fact]
        public void SavesSnips()
        {
            Assert.True(ScreenshotFilter.ShouldSave(Snip, screenshotsOnly: true));
        }

        [Fact]
        public void SkipsCopiedImagesWhenScreenshotsOnly()
        {
            Assert.False(ScreenshotFilter.ShouldSave(Browser, screenshotsOnly: true));
            Assert.False(ScreenshotFilter.ShouldSave(Word, screenshotsOnly: true));
            Assert.False(ScreenshotFilter.ShouldSave(Explorer, screenshotsOnly: true));
        }

        [Fact]
        public void SavesCopiedImagesWhenScreenshotsOnlyIsOff()
        {
            Assert.True(ScreenshotFilter.ShouldSave(Browser, screenshotsOnly: false));
            Assert.True(ScreenshotFilter.ShouldSave(Word, screenshotsOnly: false));
        }

        [Fact]
        public void NeverSavesText()
        {
            Assert.False(ScreenshotFilter.ShouldSave(Text, screenshotsOnly: false));
            Assert.False(ScreenshotFilter.ShouldSave(null, screenshotsOnly: false));
        }

        [Fact]
        public void RespectsAppsThatAskToBeSkipped()
        {
            var formats = new[] { "Bitmap", "ExcludeClipboardContentFromMonitorProcessing" };
            Assert.False(ScreenshotFilter.ShouldSave(formats, screenshotsOnly: false));
        }
    }
}
