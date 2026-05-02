# Tech Stack

## Language & Runtime

- C# on .NET 10.0
- Nullable reference types enabled (`<Nullable>enable</Nullable>`)
- Implicit usings enabled

## Frameworks & Libraries

- **WPF** (`<UseWPF>true</UseWPF>`) — UI framework for the main app
- **WinForms** (`<UseWindowsForms>true</UseWindowsForms>`) — retained solely for `FolderBrowserDialog`
- **xUnit** (v2.9.2) — unit test framework
- **FsCheck.Xunit** (v2.16.6) — property-based testing
- **Microsoft.NET.Test.Sdk** (v17.11.1) — test infrastructure

## Build System

MSBuild via .NET SDK. Solution file: `MainSolution.slnx` (modern `.slnx` format).

## Common Commands

```bash
# Build all projects
dotnet build

# Run all tests
dotnet test

# Run tests (single pass, no watch)
dotnet test --no-build

# Launch WPF app
dotnet run --project AviFileRename.Wpf

# Publish self-contained Windows executable
dotnet publish AviFileRename.Wpf -c Release -r win-x64 --self-contained
```
