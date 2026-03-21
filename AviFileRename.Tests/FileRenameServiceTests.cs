using System;
using System.IO;
using System.Collections.Generic;
using Xunit;
using AviFileRename.Core;

namespace AviFileRename.Tests
{
    public class FileRenameServiceTests
    {
        private readonly FileRenameService svc = new FileRenameService();

        // ── 9.1 Clean — noise token stripping, group suffix, separator normalisation, null/empty ──

        [Fact]
        public void Clean_Null_ReturnsEmpty()
        {
            Assert.Equal("", svc.Clean(null!));
        }

        [Fact]
        public void Clean_Empty_ReturnsEmpty()
        {
            Assert.Equal("", svc.Clean(""));
        }

        [Fact]
        public void Clean_Whitespace_ReturnsEmpty()
        {
            Assert.Equal("", svc.Clean("   "));
        }

        [Fact]
        public void Clean_NoiseToken_1080p_Stripped()
        {
            // "1080p" should be stripped; remaining tokens form the title
            var result = svc.Clean("movie.1080p.mkv.name");
            Assert.DoesNotContain("1080p", result, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Clean_GroupSuffix_Axxo_Stripped()
        {
            var result = svc.Clean("show-axxo");
            Assert.DoesNotContain("axxo", result, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Clean_Dots_ReplacedWithSpaces()
        {
            var result = svc.Clean("show.name.test");
            Assert.Equal("Show Name Test", result);
        }

        [Fact]
        public void Clean_Underscores_ReplacedWithSpaces()
        {
            var result = svc.Clean("show_name");
            Assert.Equal("Show Name", result);
        }

        // ── 9.2 Clean — TV single-episode patterns ──

        [Theory]
        [InlineData("Breaking.Bad.S05E14.720p.HDTV.x264-KILLERS", "Breaking Bad S05E14")]
        [InlineData("Firefly.1x02.The.Train.Job.HDTV", "Firefly S01E02")]
        [InlineData("Foundation.2021.S03E01.720p.x264-FENiX", "Foundation S03E01")]
        [InlineData("Some.Show.102.HDTV", "Some Show S01E02")]
        public void Clean_SingleEpisodeTv(string input, string expected)
        {
            Assert.Equal(expected, svc.Clean(input));
        }

        // ── 9.3 Clean — TV multi-episode patterns ──

        [Fact]
        public void Clean_MultiEpisodeTv()
        {
            Assert.Equal("Show Name S01E01-E03", svc.Clean("Show.Name.S01E01-E03.WEBRip"));
        }

        // ── 9.4 Clean — movie year pattern ──

        [Theory]
        [InlineData("The.Dark.Knight.2008.1080p.BluRay.x264-YIFY", "The Dark Knight (2008)")]
        [InlineData("Movie.Title.2015.720p", "Movie Title (2015)")]
        public void Clean_MovieYear(string input, string expected)
        {
            Assert.Equal(expected, svc.Clean(input));
        }

        // ── 9.5 Clean — TitleCase and word-boundary safety ──

        [Fact]
        public void Clean_OutputStartsWithUppercase()
        {
            var result = svc.Clean("breaking bad s05e14");
            Assert.True(char.IsUpper(result[0]), $"Expected uppercase first char, got: {result}");
        }

        [Fact]
        public void Clean_WebTokenDoesNotCorruptWebmaster()
        {
            // "web" noise token must not corrupt "webmaster"
            Assert.Equal("Webmaster Show S01E01", svc.Clean("webmaster.show.S01E01"));
        }

        [Fact]
        public void Clean_WebTokenDoesNotCorruptWeber()
        {
            Assert.Equal("Weber S01E01", svc.Clean("weber.S01E01"));
        }

        // ── 9.6 ScanDirectory ──

        [Fact]
        public void ScanDirectory_NonExistentPath_ReturnsEmpty()
        {
            var result = svc.ScanDirectory(@"Z:\DoesNotExist\Path", new[] { "mkv" });
            Assert.Empty(result);
        }

        [Fact]
        public void ScanDirectory_ExcludesSampleFiles()
        {
            var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "sample.movie.mkv"), "");
                var result = svc.ScanDirectory(dir, new[] { "mkv" });
                Assert.Empty(result);
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        [Fact]
        public void ScanDirectory_ExtensionHasNoDot()
        {
            var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "movie.mkv"), "");
                var result = svc.ScanDirectory(dir, new[] { "mkv" });
                Assert.Single(result);
                Assert.Equal("mkv", result[0].Extension);
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }
    }
}
