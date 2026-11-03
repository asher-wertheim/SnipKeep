using System;
using Xunit;

namespace SnipKeep.Tests
{
    public class SettingsTests
    {
        [Fact]
        public void DefaultsMatchSpec()
        {
            var s = Settings.CreateDefault();
            Assert.EndsWith(@"\Screenshots", s.Folder);
            Assert.Equal("Screenshot {yyyy-MM-dd HH.mm.ss}", s.NamePattern);
            Assert.True(s.ScreenshotsOnly);
            Assert.True(s.StartWithWindows);
            Assert.False(s.Paused);
            Assert.Equal("", s.PausedUntil);
        }

        [Fact]
        public void RoundTrips()
        {
            var s = Settings.CreateDefault();
            s.Folder = @"D:\Shots";
            s.NamePattern = "Snip {HH.mm}";
            s.ScreenshotsOnly = false;
            s.Paused = true;
            s.PausedUntilTime = new DateTime(2026, 11, 3, 14, 30, 0, DateTimeKind.Local);

            var copy = Settings.Deserialize(Settings.Serialize(s));

            Assert.Equal(@"D:\Shots", copy.Folder);
            Assert.Equal("Snip {HH.mm}", copy.NamePattern);
            Assert.False(copy.ScreenshotsOnly);
            Assert.True(copy.Paused);
            Assert.Equal(s.PausedUntilTime, copy.PausedUntilTime);
        }

        [Fact]
        public void UsesReadableKeys()
        {
            var json = Settings.Serialize(Settings.CreateDefault());
            Assert.Contains("\"folder\"", json);
            Assert.Contains("\"namePattern\"", json);
            Assert.Contains("\"screenshotsOnly\"", json);
            Assert.Contains("\"startWithWindows\"", json);
        }

        [Fact]
        public void MissingKeysGetDefaults()
        {
            var s = Settings.Deserialize("{\"folder\":\"D:\\\\Shots\"}");
            Assert.Equal(@"D:\Shots", s.Folder);
            Assert.Equal(Settings.DefaultNamePattern, s.NamePattern);
            Assert.True(s.ScreenshotsOnly);
            Assert.True(s.StartWithWindows);
        }
    }
}
