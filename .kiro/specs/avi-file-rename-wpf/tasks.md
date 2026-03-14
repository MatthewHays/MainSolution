# Implementation Plan: AviFileRename.Wpf

## Overview

Split the existing single-project WPF app into a Core library + WPF host, update `FileRenameService` with async methods and word-boundary noise matching, update the XAML and code-behind, wire up the new "Move to Dest" button, add a `StatusLabel`, register both projects in `MainSolution.slnx`, and add an xUnit + FsCheck test project.

## Tasks

- [x] 1. Create AviFileRename.Core class library project
  - Create `AviFileRename.Core/AviFileRename.Core.csproj` targeting `net8.0`
  - Move `FileEntry` out of `FileRenameService` into its own file `AviFileRename.Core/FileEntry.cs` with namespace `AviFileRename.Core`
  - Move `FileRenameService.cs` to `AviFileRename.Core/FileRenameService.cs`, update namespace to `AviFileRename.Core`
  - _Requirements: 1.1, 1.3, 1.4_

- [x] 2. Update FileRenameService with async methods and word-boundary noise matching
  - [x] 2.1 Make `Clean` an instance method (remove `static`), update `ScanDirectory` call site accordingly
    - _Requirements: 1.3_

  - [x] 2.2 Replace `string.Replace` noise-token stripping with `Regex.Replace` using `\b<token>\b` word-boundary pattern (compiled, case-insensitive)
    - Apply to all tokens in `NoiseTokens`
    - _Requirements: 3.2, 3.12_

  - [ ]* 2.3 Write property test: word-boundary safety (Property 9)
    - **Property 9: Word-boundary safety — tokens do not corrupt longer words**
    - **Validates: Requirement 3.12**

  - [x] 2.4 Add `RenameFilesAsync(List<FileEntry> files) → Task<int>` replacing the synchronous `RenameFiles`
    - Use `await Task.Run(...)` for each `File.Move`
    - Skip if destination already exists; co-rename `.srt` when present
    - Return count of files actually renamed
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5_

  - [ ]* 2.5 Write property test: RenameFilesAsync never overwrites an existing file (Property 5)
    - **Property 5: RenameFilesAsync never overwrites an existing file**
    - **Validates: Requirement 4.2**

  - [x] 2.6 Add `CollapseAsync(string source, string destination, SearchOption searchOption, string[] extensions) → Task<int>`
    - Enumerate files per extension using `DirectoryInfo.GetFiles`, move each with `await Task.Run(...)`
    - Silently catch per-file exceptions; return count of successfully moved files
    - _Requirements: 5.2, 5.3, 5.4, 5.5, 6.2, 6.3_

  - [ ]* 2.7 Write property test: Clean is idempotent (Property 1)
    - **Property 1: Clean is idempotent**
    - **Validates: Requirement 3.9**

  - [ ]* 2.8 Write property test: Clean never returns null (Property 2)
    - **Property 2: Clean never returns null**
    - **Validates: Requirement 3.10**

  - [ ]* 2.9 Write property test: no noise token survives cleaning as a whole word (Property 3)
    - **Property 3: No noise token survives cleaning as a whole word**
    - **Validates: Requirement 3.2**

  - [ ]* 2.10 Write property test: Clean output contains no raw separator characters (Property 4)
    - **Property 4: Clean output contains no raw separator characters**
    - **Validates: Requirement 3.11**

  - [ ]* 2.11 Write property test: TV episode output matches canonical pattern (Property 7)
    - **Property 7: TV episode output matches canonical pattern**
    - **Validates: Requirements 3.5, 3.6**

  - [ ]* 2.12 Write property test: Movie output wraps year in parentheses (Property 8)
    - **Property 8: Movie output wraps year in parentheses**
    - **Validates: Requirement 3.7**

- [x] 3. Checkpoint — ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [x] 4. Update AviFileRename.Wpf to reference Core and remove old FileRenameService.cs
  - Add `<ProjectReference>` to `AviFileRename.Core` in `AviFileRename.Wpf.csproj`
  - Delete `AviFileRename.Wpf/FileRenameService.cs`
  - Verify `using AviFileRename.Core;` is present in `MainWindow.xaml.cs`
  - _Requirements: 1.1, 1.2_

- [x] 5. Update MainWindow.xaml layout
  - Set `Height="200"` and `ResizeMode="CanResizeWithGrip"` on the `<Window>`
  - Ensure the destination row (`DestTextBox` + `DestBrowseButton`) is visible and not commented out
  - Rename buttons: `RenameAllButton` → `RenameFilesButton` (Content="Rename Files"), `CollapseFlattenButton` → `FlattenFolderButton` (Content="Flatten Folder"), add `MoveToDestButton` (Content="Move to Dest")
  - Add `<Label x:Name="StatusLabel">` below the button row spanning full width (`Grid.ColumnSpan="2"`)
  - Add `ToolTip="Select source folder"` to `SourceBrowseButton`, `ToolTip="Select destination folder"` to `DestBrowseButton`, `ToolTip="Open source folder in Explorer"` to `OpenFolderButton`
  - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5, 7.6_

- [x] 6. Update MainWindow.xaml.cs event handlers and helpers
  - [x] 6.1 Add `SetBusy(bool busy)` helper: disables/enables all operation buttons and toggles `Cursor` between `Cursors.Wait` and `Cursors.Arrow`
    - _Requirements: 8.1, 8.2, 8.3_

  - [x] 6.2 Add `SetStatus(string message)` helper: sets `StatusLabel.Content`
    - _Requirements: 7.3_

  - [x] 6.3 Replace `RenameAllButton_Click` with `async void RenameFilesButton_Click`
    - Call `SetBusy(true)`, guard for empty scan result (set status "No video files found in \<path\>" and return), call `await Task.Run(() => svc.RenameFilesAsync(...))`, call `SetStatus("Renamed N files.")`, wrap in try/catch for `MessageBox` on unexpected exception, call `SetBusy(false)` in finally
    - _Requirements: 4.1, 4.4, 8.1, 8.2, 8.4, 10.1_

  - [x] 6.4 Replace `CollapseFlattenButton_Click` with `async void FlattenFolderButton_Click`
    - Call `CollapseAsync` with `source, source, SearchOption.AllDirectories, ["avi","mkv","mp4","srt"]`
    - Set status "Flatten complete. N files moved."
    - _Requirements: 5.1, 5.6, 8.1, 8.2_

  - [x] 6.5 Add `async void MoveToDestButton_Click`
    - Call `CollapseAsync` with `source, dest, SearchOption.TopDirectoryOnly, ["avi","mkv","mp4","srt"]`
    - Set status "Move to Dest complete. N files moved."
    - _Requirements: 6.1, 6.4, 8.1, 8.2_

  - [x] 6.6 Remove the private `Collapse()` helper method from `MainWindow.xaml.cs`
    - _Requirements: 5.2 (logic now in FileRenameService)_

  - [ ]* 6.7 Write unit tests for empty-scan guard and SetBusy/SetStatus helpers
    - Test that `RenameFilesButton_Click` sets the correct status message when scan returns empty
    - _Requirements: 10.1_

- [x] 7. Checkpoint — ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [x] 8. Create AviFileRename.Tests xUnit + FsCheck project
  - Create `AviFileRename.Tests/AviFileRename.Tests.csproj` targeting `net8.0`
  - Add NuGet references: `xunit`, `xunit.runner.visualstudio`, `FsCheck.Xunit`
  - Add `<ProjectReference>` to `AviFileRename.Core`
  - _Requirements: (testing infrastructure)_

- [x] 9. Write unit tests for FileRenameService
  - [x] 9.1 Write unit tests for `Clean` — noise token stripping, group suffix stripping, separator normalization, null/empty input, already-clean input
    - _Requirements: 3.1, 3.2, 3.3, 3.4_

  - [x] 9.2 Write unit tests for `Clean` — TV single-episode patterns (S01E02, 1x02, packed 102)
    - _Requirements: 3.5_

  - [x] 9.3 Write unit tests for `Clean` — TV multi-episode patterns (S01E01-E03)
    - _Requirements: 3.6_

  - [x] 9.4 Write unit tests for `Clean` — movie year pattern
    - _Requirements: 3.7_

  - [x] 9.5 Write unit tests for `Clean` — TitleCase output, word-boundary safety ("webmaster", "weber")
    - _Requirements: 3.8, 3.12_

  - [x] 9.6 Write unit tests for `ScanDirectory` — non-existent directory returns empty list, "sample" files excluded, extensions set correctly
    - _Requirements: 2.2, 2.3, 2.5_

  - [ ]* 9.7 Write property test: ScanDirectory returns empty list for non-existent directory (Property 10)
    - **Property 10: ScanDirectory returns empty list for non-existent directory**
    - **Validates: Requirement 2.3**

  - [ ]* 9.8 Write property test: ScanDirectory excludes sample files (Property 6)
    - **Property 6: ScanDirectory excludes sample files**
    - **Validates: Requirement 2.2**

- [x] 10. Add projects to MainSolution.slnx
  - Add `<Project Path="AviFileRename.Core/AviFileRename.Core.csproj" />` to `MainSolution.slnx`
  - Add `<Project Path="AviFileRename.Tests/AviFileRename.Tests.csproj" />` to `MainSolution.slnx`
  - _Requirements: 1.1_

- [x] 11. Final checkpoint — ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for a faster MVP
- Each task references specific requirements for traceability
- Property tests use FsCheck.Xunit; unit tests use xUnit
- `CollapseAsync` replaces the former `MainWindow.Collapse()` helper — `MainWindow` must have no direct `File.Move` calls after task 6.6
