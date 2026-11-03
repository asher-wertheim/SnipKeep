using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace SnipKeep.Tests
{
    public class FileNamerTests
    {
        private static readonly DateTime Time = new DateTime(2026, 11, 3, 12, 30, 49);

        [Fact]
        public void DefaultPatternMatchesSpec()
        {
            Assert.Equal("Screenshot 2026-11-03 12.30.49", FileNamer.Format(Settings.DefaultNamePattern, Time));
        }

        [Fact]
        public void RemovesCharactersWindowsDoesNotAllow()
        {
            Assert.Equal("Shot 2026-11-03 123049", FileNamer.Format("Shot {yyyy-MM-dd HH:mm:ss}", Time));
            Assert.Equal("ab", FileNamer.Format("a<>|?*\"b", Time));
        }

        [Fact]
        public void TrimsTrailingDotsAndSpaces()
        {
            Assert.Equal("Shot", FileNamer.Format("Shot. . ", Time));
        }

        [Fact]
        public void EmptyResultFallsBackToScreenshot()
        {
            Assert.Equal("Screenshot", FileNamer.Format("???", Time));
        }

        [Theory]
        [InlineData("Shot {yyyy")]
        [InlineData("Shot }")]
        public void UnbalancedBracesAreNotValid(string pattern)
        {
            Assert.Null(FileNamer.Format(pattern, Time));
            Assert.Equal("Screenshot 2026-11-03 12.30.49", FileNamer.FormatOrDefault(pattern, Time));
        }

        [Fact]
        public void UniquePathAddsNumberOnClash()
        {
            var taken = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                Path.Combine(@"C:\Shots", "Shot.png"),
                Path.Combine(@"C:\Shots", "Shot (2).png"),
            };

            Assert.Equal(Path.Combine(@"C:\Shots", "Shot (3).png"), FileNamer.UniquePath(@"C:\Shots", "Shot", taken.Contains));
            Assert.Equal(Path.Combine(@"C:\Shots", "Other.png"), FileNamer.UniquePath(@"C:\Shots", "Other", taken.Contains));
        }
    }
}
