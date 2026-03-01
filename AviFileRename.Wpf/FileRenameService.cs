using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;

namespace AviFileRename.Core
{
    public class FileRenameService
    {
        public class FileEntry
        {
            public string OriginalPath { get; set; } = string.Empty;
            public string OriginalName { get; set; } = string.Empty;
            public string SuggestedName { get; set; } = string.Empty;
            public string Extension { get; set; } = string.Empty;
        }

        private static readonly string[] NoiseTokens =
        {
            "1080p", "720p", "2160p", "4k",
            "x264", "h264", "x265", "h265", "hevc",
            "webrip", "web-dl", "webdl", "web", "hdrip",
            "dvdrip", "bdrip", "brrip", "hdtv", "bluray", "blueray", "bdremux",
            "xvid",
            "yify", "ettv", "eztv", "rarbg", "tgx", "evo", "ntb", "amzn", "nf",
            "proper", "repack", "extended", "remux"
        };

        private static readonly string[] GroupSuffixes =
        {
            "-axxo", "-lol", "-2hd", "-fov", "-bia", "-fqm", "-notv", "-pow4", "-killers", "-evolve"
        };

        private static readonly Regex MultiWhitespaceRegex =
            new Regex(@"\s+", RegexOptions.Compiled);

        // Matches multi-episode TV patterns, for example:
        // - "Show.Name.S01E01-E02.1080p.WEB-DL.x264-GROUP"
        // - "Show Name - S01E01-02 - Episode Title - 720p HDTV x264-GROUP"
        private static readonly Regex MultiEpisodeTvRegex =
            new Regex(
                @"^(?<title>.+?)\s*(?:[-. ]+)?s(?<season>\d{1,2})e(?<ep1>\d{2})[-E](?<ep2>\d{2})(?:\s+[-. ]*(?<extra>.*))?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Matches single-episode TV patterns, for example:
        // - "Show.Name.S01E02.1080p.WEB-DL.DD5.1.H.264-GROUP"
        // - "Show.Name.1x02.HDTV.XviD-GROUP"
        // - "Show.Name.102.HDTV.x264-GROUP" (102 => season 1, episode 02)
        // Optional "extra" text at the end becomes " - Extra" in the final name.
        private static readonly Regex SingleEpisodeTvRegex =
            new Regex(
                @"^(?<title>.+?)\s*(?:[-. ]+)?(?:
                      s(?<season>\d{1,2})e(?<episode>\d{1,2}) |
                      (?<season>\d{1,2})x(?<episode>\d{2})   |
                      (?<packed>\d{3})
                  )(?:\s+[-. ]*(?<extra>.*))?$",
                RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        // Matches movie patterns with a year, for example:
        // - "Movie.Title.2015.1080p.BluRay.x264-GROUP"
        // - "Movie Title (2015) 720p WEB-DL x265-GROUP"
        // The result becomes "Movie Title (2015)".
        private static readonly Regex MovieRegex =
            new Regex(
                @"^(?<title>.+?)\s*\(?(?<year>\d{4})\)?(?:\s+.*)?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public List<FileEntry> ScanDirectory(string sourceDirectory, string[] extensions)
        {
            var files = new List<FileEntry>();
            if (!Directory.Exists(sourceDirectory))
                return files;

            foreach (var ext in extensions)
            {
                foreach (var file in Directory.GetFiles(sourceDirectory, "*." + ext, SearchOption.AllDirectories))
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    if (fileName.ToLower().Contains("sample"))
                        continue;
                    files.Add(new FileEntry
                    {
                        OriginalPath = file,
                        OriginalName = fileName,
                        SuggestedName = Clean(fileName),
                        Extension = ext
                    });
                }
            }
            return files;
        }

        public static string Clean(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            var text = name.ToLowerInvariant();

            foreach (var token in NoiseTokens)
            {
                text = text.Replace(token, " ");
            }

            foreach (var suffix in GroupSuffixes)
            {
                text = text.Replace(suffix, " ");
            }

            text = text
                .Replace("[eng]", " ")
                .Replace("series", "S")
                .Replace("season", "S")
                .Replace("episode", "E")
                .Replace(".", " ")
                .Replace("_", " ")
                .Replace("[", "(")
                .Replace("]", ")")
                .Replace("cd1", "1-2")
                .Replace("cd2", "2-2");

            text = MultiWhitespaceRegex.Replace(text, " ").Trim();

            var normalized = NormalizeShowOrMovie(text);

            normalized = normalized.Replace("((", "(")
                                   .Replace("))", ")")
                                   .Replace("()", " ");

            normalized = MultiWhitespaceRegex.Replace(normalized, " ").Trim();

            normalized = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(normalized);

            return normalized.Trim();
        }

        private static string NormalizeShowOrMovie(string input)
        {
            var multi = MultiEpisodeTvRegex.Match(input);
            if (multi.Success)
            {
                var title = multi.Groups["title"].Value.Trim();
                var season = int.Parse(multi.Groups["season"].Value);
                var ep1 = int.Parse(multi.Groups["ep1"].Value);
                var ep2 = int.Parse(multi.Groups["ep2"].Value);
                var extra = multi.Groups["extra"].Value.Trim();

                var baseName = $"{title} S{season:00}E{ep1:00}-E{ep2:00}";
                if (!string.IsNullOrEmpty(extra))
                {
                    baseName += " - " + extra;
                }

                return baseName;
            }

            var tv = SingleEpisodeTvRegex.Match(input);
            if (tv.Success)
            {
                var title = tv.Groups["title"].Value.Trim();
                int season;
                int episode;

                if (tv.Groups["packed"].Success)
                {
                    var packed = int.Parse(tv.Groups["packed"].Value);
                    season = packed / 100;
                    episode = packed % 100;
                }
                else
                {
                    season = int.Parse(tv.Groups["season"].Value);
                    episode = int.Parse(tv.Groups["episode"].Value);
                }

                var extra = tv.Groups["extra"].Value.Trim();

                var baseName = $"{title} S{season:00}E{episode:00}";
                if (!string.IsNullOrEmpty(extra))
                {
                    baseName += " - " + extra;
                }

                return baseName;
            }

            var movie = MovieRegex.Match(input);
            if (movie.Success)
            {
                var title = movie.Groups["title"].Value.Trim();
                var year = movie.Groups["year"].Value;
                return $"{title} ({year})";
            }

            return input;
        }

        public void RenameFiles(List<FileEntry> files)
        {
            foreach (var entry in files)
            {
                var dir = Path.GetDirectoryName(entry.OriginalPath);
                var oldName = Path.GetFileNameWithoutExtension(entry.OriginalPath);
                var newPath = Path.Combine(dir ?? string.Empty, entry.SuggestedName + "." + entry.Extension);
                if (entry.OriginalPath != newPath && !File.Exists(newPath))
                {
                    File.Move(entry.OriginalPath, newPath);
                    // Rename subtitle if exists
                    var oldSub = Path.Combine(dir ?? string.Empty, oldName + ".srt");
                    var newSub = Path.Combine(dir ?? string.Empty, entry.SuggestedName + ".srt");
                    if (File.Exists(oldSub))
                    {
                        File.Move(oldSub, newSub);
                    }
                }
            }
        }
    }
}
