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
        MoveToDestButton.IsEnabled = !busy;
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
            int count = await _renameService.RenameFilesAsync(files);
            SetStatus($"Renamed {count} files.");
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
