namespace AviFileRename.Core
{
    public class FileEntry
    {
        public string OriginalPath { get; set; } = string.Empty;
        public string OriginalName { get; set; } = string.Empty;
        public string SuggestedName { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
    }
}
