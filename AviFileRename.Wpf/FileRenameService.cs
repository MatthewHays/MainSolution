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
            name = name.ToLower();
            name = name.Replace("1080p", "");
            name = name.Replace("720p", "");
            name = name.Replace("x264", "");
            name = name.Replace("h264", "");
            name = name.Replace("[eng]", "");
            name = name.Replace("-axxo", "");
            name = name.Replace("-lol", "");
            name = name.Replace("-2hd", "");
            name = name.Replace("-fov", "");
            name = name.Replace("-bia", "");
            name = name.Replace("-fqm", "");
            name = name.Replace("-notv", "");
            name = name.Replace("-pow4", "");
            name = name.Replace("-killers", "");
            name = name.Replace("-evolve", "");
            name = name.Replace("yify", "");
            name = name.Replace("ettv", "");
            name = name.Replace("webrip", "");
            name = name.Replace("dvdrip", "");
            name = name.Replace("xvid", "");
            name = name.Replace("bdrip", "");
            name = name.Replace("hdtv", "");
            name = name.Replace("blueray", "");
            name = name.Replace("bluray", "");
            name = name.Replace("brrip", "");
            name = name.Replace("series", "S");
            name = name.Replace("season", "S");
            name = name.Replace("episode", "E");
            name = name.Replace(".", " ");
            name = name.Replace("_", " ");
            name = name.Replace("[", "(");
            name = name.Replace("]", ")");
            name = name.Replace("cd1", "1-2");
            name = name.Replace("cd2", "2-2");
            name = name.Replace("  ", " ");
            name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name);
            name = Regex.Replace(name, @"(\(?[0-9]{4}\)?).*", " ($1)");
            name = Regex.Replace(name, @"\(+([0-9])([0-9][0-9])\)+", "S0$1E$2");
            name = Regex.Replace(name, @"\(+([0-9][0-9])x([0-9][0-9])\)+", "S$1E$2", RegexOptions.IgnoreCase);
            name = Regex.Replace(name, @"\(+([0-9])x([0-9][0-9])\)+", "S0$1E$2", RegexOptions.IgnoreCase);
            name = Regex.Replace(name, " S ([0-9])", " S$1", RegexOptions.IgnoreCase);
            name = Regex.Replace(name, "([0-9]) E ([0-9])", "$1E$2", RegexOptions.IgnoreCase);
            name = Regex.Replace(name, " S([0-9])E", " S0$1E", RegexOptions.IgnoreCase);
            name = Regex.Replace(name, "E([0-9])$", "E0$1", RegexOptions.IgnoreCase);
            name = Regex.Replace(name, "S([0-9][0-9])E([0-9][0-9]).*", "S$1E$2", RegexOptions.IgnoreCase);
            name = Regex.Replace(name, "- S([0-9][0-9])E([0-9][0-9])", " S$1E$2", RegexOptions.IgnoreCase);
            name = name.Replace("((", "(");
            name = name.Replace("))", ")");
            name = name.Replace("()", "");
            name = name.Replace("  ", " ");
            return name.Trim();
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
