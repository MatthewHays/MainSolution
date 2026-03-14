# Requirements Document

## Introduction

AviFileRename.Wpf is a WPF desktop utility (net8.0-windows) for batch-renaming and organizing video files. It replaces the original WinForms per-file interactive approval dialog with fully automated batch processing. The application applies a regex-based filename normalization pipeline to produce clean, consistently formatted names for movies and TV episodes. It exposes four operations: batch rename in place, flatten a nested folder tree into the source root, move top-level files to a destination folder, and open the source folder in Explorer.

## Glossary

- **Application**: The AviFileRename.Wpf WPF desktop application
- **FileRenameService**: The instance service class in AviFileRename.Core that encapsulates all filename analysis, renaming, and folder-collapse logic
- **MainWindow**: The single-screen WPF UI window
- **FileEntry**: A data record holding the original path, original name, suggested name, and extension for a scanned video file
- **Clean**: The filename normalization pipeline method on FileRenameService
- **ScanDirectory**: The method that enumerates video files in a directory and builds a list of FileEntry records
- **RenameFilesAsync**: The method that applies rename operations from a list of FileEntry records
- **CollapseAsync**: The method that moves files from a source directory to a destination directory
- **NoiseTokens**: The set of release-group and encoding tokens stripped from filenames during normalization
- **GroupSuffixes**: The set of release-group suffix strings stripped from filenames during normalization
- **StatusLabel**: The UI label that displays operation results inline
- **SetBusy**: The UI helper that disables buttons and sets the wait cursor during an operation

## Requirements

### Requirement 1: Project Structure

**User Story:** As a developer, I want the application split into a Core library and a WPF host, so that the file operation logic is independently testable and reusable.

#### Acceptance Criteria

1. THE Application SHALL consist of two projects: AviFileRename.Core (a net8.0 class library) and AviFileRename.Wpf (a net8.0-windows WPF application)
2. THE AviFileRename.Wpf project SHALL reference AviFileRename.Core
3. THE FileRenameService SHALL be defined in AviFileRename.Core with all public methods as instance methods
4. THE FileEntry class SHALL be defined in AviFileRename.Core as part of its public API

### Requirement 2: Directory Scanning

**User Story:** As a user, I want the application to scan a folder for video files, so that I can batch-rename all eligible files at once.

#### Acceptance Criteria

1. WHEN ScanDirectory is called with a source directory and an extensions array, THE FileRenameService SHALL return a list of FileEntry records for all files matching those extensions found recursively in the directory
2. WHEN ScanDirectory is called, THE FileRenameService SHALL exclude any file whose name contains the substring "sample" (case-insensitive)
3. WHEN ScanDirectory is called on a directory that does not exist, THE FileRenameService SHALL return an empty list rather than throwing an exception
4. WHEN ScanDirectory returns entries, THE FileRenameService SHALL set each FileEntry's SuggestedName to the result of Clean applied to the OriginalName
5. THE FileRenameService SHALL set each FileEntry's Extension to a lowercase string with no leading dot

### Requirement 3: Filename Normalization (Clean)

**User Story:** As a user, I want filenames normalized into a consistent format, so that my video library has clean, readable names.

#### Acceptance Criteria

1. WHEN Clean is called with a null or whitespace-only string, THE FileRenameService SHALL return an empty string
2. THE FileRenameService SHALL strip each token in NoiseTokens from the filename using word-boundary regex matching (case-insensitive)
3. THE FileRenameService SHALL strip each suffix in GroupSuffixes from the filename as a literal string
4. THE FileRenameService SHALL replace separator characters (`.`, `_`, `[`, `]`) with spaces during normalization
5. WHEN a filename matches a single-episode TV pattern (e.g. S01E02, 1x02, or packed 102), THE FileRenameService SHALL format the output as "Title S##E##" with zero-padded season and episode numbers
6. WHEN a filename matches a multi-episode TV pattern (e.g. S01E01-E03), THE FileRenameService SHALL format the output as "Title S##E##-E##" with zero-padded numbers
7. WHEN a filename matches a movie pattern containing a four-digit year, THE FileRenameService SHALL format the output as "Title (YYYY)"
8. THE FileRenameService SHALL apply TitleCase to the final normalized output
9. THE FileRenameService Clean method SHALL be idempotent: applying Clean twice to any input SHALL produce the same result as applying it once
10. THE FileRenameService Clean method SHALL never return null
11. THE FileRenameService Clean method SHALL produce output that contains no separator characters (`.`, `_`, `[`, `]`)
12. THE FileRenameService SHALL NOT corrupt words that contain a noise token as a substring but not as a whole word (e.g. "webmaster" SHALL NOT be altered by the "web" noise token)

### Requirement 4: Batch File Rename

**User Story:** As a user, I want to rename all scanned video files in one click, so that I don't have to rename each file manually.

#### Acceptance Criteria

1. WHEN RenameFilesAsync is called with a list of FileEntry records, THE FileRenameService SHALL move each file to a path formed by combining the file's original directory, its SuggestedName, and its Extension
2. WHEN the computed destination path already exists on disk, THE FileRenameService SHALL skip that file and leave the existing file unchanged
3. WHEN a video file is renamed and a matching .srt subtitle file exists in the same directory with the same base name, THE FileRenameService SHALL rename the subtitle file to match the new video filename
4. THE RenameFilesAsync method SHALL return the count of files actually renamed, excluding skipped files
5. THE RenameFilesAsync method SHALL execute file operations on a background thread so that the UI thread is not blocked

### Requirement 5: Flatten Folder

**User Story:** As a user, I want to flatten a nested folder tree into the source root, so that all video files are in one place for easy access.

#### Acceptance Criteria

1. WHEN the user clicks "Flatten Folder", THE Application SHALL call CollapseAsync with the source directory as both source and destination and SearchOption.AllDirectories
2. WHEN CollapseAsync is called with SearchOption.AllDirectories, THE FileRenameService SHALL move all files matching the specified extensions from all subdirectories into the destination root directory
3. WHEN a file move fails during CollapseAsync (e.g. duplicate filename, locked file), THE FileRenameService SHALL silently skip that file and continue processing remaining files
4. THE CollapseAsync method SHALL return the count of files successfully moved
5. THE CollapseAsync method SHALL execute file operations on a background thread so that the UI thread is not blocked
6. WHEN Flatten Folder completes, THE Application SHALL display "Flatten complete. N files moved." in the StatusLabel

### Requirement 6: Move to Destination

**User Story:** As a user, I want to move top-level video files from a source folder to a destination folder, so that I can organize my downloads into my media library.

#### Acceptance Criteria

1. WHEN the user clicks "Move to Dest", THE Application SHALL call CollapseAsync with the source directory, the destination directory, and SearchOption.TopDirectoryOnly
2. WHEN CollapseAsync is called with SearchOption.TopDirectoryOnly, THE FileRenameService SHALL move only files in the top-level of the source directory to the destination root, leaving files in subdirectories untouched
3. WHEN a file move fails during CollapseAsync, THE FileRenameService SHALL silently skip that file and continue processing remaining files
4. WHEN Move to Dest completes, THE Application SHALL display "Move to Dest complete. N files moved." in the StatusLabel

### Requirement 7: UI Controls and Layout

**User Story:** As a user, I want a clear, functional UI with source and destination path inputs and clearly labelled action buttons, so that I can operate the tool without confusion.

#### Acceptance Criteria

1. THE MainWindow SHALL display a SourceTextBox with a default value of "D:\Downloads" and a DestTextBox with a default value of "D:\Videos\Movies"
2. THE MainWindow SHALL display three operation buttons labelled "Rename Files", "Flatten Folder", and "Move to Dest"
3. THE MainWindow SHALL display a StatusLabel below the button row spanning the full window width to show operation results
4. THE MainWindow SHALL display browse buttons for both source and destination folders with tooltips "Select source folder" and "Select destination folder" respectively
5. THE MainWindow SHALL display an "Open Folder" button with tooltip "Open source folder in Explorer"
6. THE MainWindow Window Height SHALL be approximately 200 pixels with ResizeMode set to CanResizeWithGrip

### Requirement 8: UI Responsiveness and Busy State

**User Story:** As a user, I want the UI to remain responsive during file operations and prevent accidental double-clicks, so that I have a smooth and safe experience.

#### Acceptance Criteria

1. WHEN a file operation begins, THE Application SHALL disable all operation buttons and set the cursor to the wait cursor
2. WHEN a file operation completes or fails, THE Application SHALL re-enable all operation buttons and restore the default cursor
3. WHILE an operation is in progress, THE Application SHALL ignore clicks on operation buttons
4. WHEN an unexpected exception occurs during an operation, THE Application SHALL display the exception message in a MessageBox

### Requirement 9: Folder Browsing and Explorer Integration

**User Story:** As a user, I want to browse for folders using a dialog and open the source folder in Explorer, so that I can easily navigate my file system.

#### Acceptance Criteria

1. WHEN the user clicks the source browse button, THE Application SHALL open a FolderBrowserDialog for folder selection and update SourceTextBox with the selected path
2. WHEN the user clicks the destination browse button, THE Application SHALL open a FolderBrowserDialog for folder selection and update DestTextBox with the selected path
3. WHEN the user cancels the FolderBrowserDialog, THE Application SHALL leave the corresponding TextBox text unchanged
4. WHEN the user clicks "Open Folder", THE Application SHALL open the path in SourceTextBox using Windows Explorer via Process.Start with UseShellExecute set to true
5. IF Process.Start throws an exception when opening the folder, THEN THE Application SHALL display the exception message in a MessageBox

### Requirement 10: Empty Scan Result Handling

**User Story:** As a user, I want clear feedback when no video files are found, so that I know the operation did not silently fail.

#### Acceptance Criteria

1. WHEN ScanDirectory returns an empty list, THE Application SHALL display "No video files found in \<path\>" in the StatusLabel and SHALL NOT attempt to call RenameFilesAsync
