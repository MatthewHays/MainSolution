using System;
using System.IO;
using System.Windows;
using AviFileRename.Core;
using System.Windows.Forms;

namespace AviFileRename.Wpf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly FileRenameService _renameService = new();

    private static readonly string[] VideoExtensions = { "avi", "mkv", "mp4" };
    private static readonly string[] CollapseExtensions = { "avi", "mkv", "mp4", "srt" };

    public MainWindow()
    {
        InitializeComponent();
    }

    private void SetBusy(bool busy)
    {
        RenameFilesButton.IsEnabled = !busy;
        FlattenFolderButton.IsEnabled = !busy;
        //MoveToDestButton.IsEnabled = !busy;
        Cursor = busy ? System.Windows.Input.Cursors.Wait : System.Windows.Input.Cursors.Arrow;
    }

    private void SetStatus(string message)
    {
        StatusLabel.Content = message;
    }

    private void SourceBrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new FolderBrowserDialog();
        dialog.SelectedPath = SourceTextBox.Text;
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            SourceTextBox.Text = dialog.SelectedPath;
        }
    }

    private void DestBrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new FolderBrowserDialog();
        dialog.SelectedPath = DestTextBox.Text;
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            DestTextBox.Text = dialog.SelectedPath;
        }
    }

    private async void RenameFilesButton_Click(object sender, RoutedEventArgs e)
    {
        var source = SourceTextBox.Text;
        SetBusy(true);
        try
        {
            var files = _renameService.ScanDirectory(source, VideoExtensions);
            if (files.Count == 0)
            {
                SetStatus($"No video files found in {source}");
                return;
            }

            int totalRenamed = 0;
            foreach (var entry in files)
            {
                // Prompt the user to confirm and optionally edit the suggested name
                var newName = PromptForFileRename(entry);
                if (newName == null)
                {
                    // user cancelled for this file; skip it
                    continue;
                }

                // Update the suggested name with user's input and perform rename
                entry.SuggestedName = newName;
                var renamed = await _renameService.RenameFileAsync(entry);
                if (renamed) totalRenamed++;
            }

            SetStatus($"Renamed {totalRenamed} files.");
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private string? PromptForFileRename(AviFileRename.Core.FileEntry entry)
    {
        // Use a simple WinForms dialog to allow the user to confirm or edit the suggested name
        using var form = new System.Windows.Forms.Form()
        {
            Width = 560,
            Height = 170,
            Text = "Confirm Rename",
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        };

        var label = new System.Windows.Forms.Label()
        {
            Left = 10,
            Top = 10,
            Width = 520,
            Text = $"Original: {Path.GetFileName(entry.OriginalPath)}"
        };

        var tb = new System.Windows.Forms.TextBox()
        {
            Left = 10,
            Top = 35,
            Width = 520,
            Text = entry.SuggestedName
        };

        var ok = new System.Windows.Forms.Button()
        {
            Text = "OK",
            Left = 350,
            Width = 80,
            Top = 70,
            DialogResult = System.Windows.Forms.DialogResult.OK
        };

        var cancel = new System.Windows.Forms.Button()
        {
            Text = "Cancel",
            Left = 440,
            Width = 80,
            Top = 70,
            DialogResult = System.Windows.Forms.DialogResult.Cancel
        };

        form.Controls.AddRange(new System.Windows.Forms.Control[] { label, tb, ok, cancel });
        form.AcceptButton = ok;
        form.CancelButton = cancel;

        var result = form.ShowDialog();
        if (result == System.Windows.Forms.DialogResult.OK)
        {
            var candidate = tb.Text?.Trim() ?? string.Empty;
            return candidate;
        }

        return null;
    }

    private async void FlattenFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var source = SourceTextBox.Text;
        SetBusy(true);
        try
        {
            int count = await _renameService.CollapseAsync(source, source, SearchOption.AllDirectories, CollapseExtensions);
            SetStatus($"Flatten complete. {count} files moved.");
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void MoveToDestButton_Click(object sender, RoutedEventArgs e)
    {
        var source = SourceTextBox.Text;
        var dest = DestTextBox.Text;
        SetBusy(true);
        try
        {
            int count = await _renameService.CollapseAsync(source, dest, SearchOption.TopDirectoryOnly, CollapseExtensions);
            SetStatus($"Move to Dest complete. {count} files moved.");
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo { FileName = SourceTextBox.Text, UseShellExecute = true };
            System.Diagnostics.Process.Start(psi);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Failed to open folder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
