# Product: AviFileRename

A Windows desktop utility for batch-renaming and organizing video files. It applies a regex-based filename normalization pipeline to produce clean, consistently formatted names for movies and TV episodes.

## Core Operations

- **Batch Rename**: Normalizes video filenames in a source folder (strips noise tokens, formats as "Title S##E##" or "Title (YYYY)")
- **Flatten Folder**: Moves all files from nested subdirectories into the source root
- **Move to Destination**: Moves top-level files to a separate destination folder
- **Explorer Integration**: Opens the source folder in Windows Explorer

## Supported File Types

- Video: `.avi`, `.mkv`, `.mp4`
- Subtitles: `.srt` (co-renamed alongside matching video files)

## Key Behaviors

- Subtitle files are automatically renamed to match their paired video file
- Files containing "sample" in the name are excluded from scanning
- Operations are non-destructive: skips rename if destination already exists
- All file operations run asynchronously to keep the UI responsive
- Status feedback is shown inline via a status label (not modal dialogs)
