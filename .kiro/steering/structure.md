# Project Structure

## Solution Layout

```
AviFileRename.Core/          # Business logic class library (net10.0)
  FileRenameService.cs       # All file scanning, renaming, and normalization logic
  FileEntry.cs               # Data model: OriginalPath, OriginalName, SuggestedName, Extension

AviFileRename.Wpf/           # WPF desktop application (net10.0-windows)
  MainWindow.xaml/.cs        # Single-screen UI with event handlers
  ConfirmRenameWindow.xaml   # Per-file rename confirmation dialog
  App.xaml/.cs               # Application entry point
  Resources/                 # Icons and embedded resources
  Tools/                     # UI utility classes

AviFileRename.Tests/         # xUnit test project (net10.0)
  FileRenameServiceTests.cs  # Tests for Clean(), ScanDirectory(), RenameFilesAsync()

AviFileRename.Winforms/      # Legacy WinForms version (do not extend)

.kiro/specs/avi-file-rename-wpf/  # Spec documents
  requirements.md
  design.md
  tasks.md
```

## Architecture Rules

- `AviFileRename.Core` has no UI dependencies — keep it that way
- `AviFileRename.Wpf` references Core; Core does not reference Wpf
- All public methods on `FileRenameService` are instance methods (not static)
- One public class per file

## Code Conventions

- PascalCase for classes, methods, properties; camelCase for locals and parameters
- Async file operations use `async Task<T>`; UI event handlers use `async void`
- Static readonly compiled regexes with named capture groups (`(?<title>...)`)
- `ScanDirectory` returns empty list (never throws) for missing directories
- `CollapseAsync` silently skips per-file failures via try-catch

## Testing Conventions

- Test method naming: `MethodName_Scenario_ExpectedResult`
- `[Fact]` for single cases, `[Theory]` + `[InlineData]` for parameterized cases
- Property-based tests use FsCheck.Xunit — key property: `Clean(Clean(x)) == Clean(x)`
- All new `FileRenameService` logic should have corresponding tests
