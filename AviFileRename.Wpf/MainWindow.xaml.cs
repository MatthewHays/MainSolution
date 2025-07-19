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

    public MainWindow()
    {
        InitializeComponent();
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

    private void RenameAllButton_Click(object sender, RoutedEventArgs e)
    {
        var files = _renameService.ScanDirectory(SourceTextBox.Text, new[] { "avi", "mkv", "mp4" });
        _renameService.RenameFiles(files);
        System.Windows.MessageBox.Show("Renaming complete.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void CollapseFlattenButton_Click(object sender, RoutedEventArgs e)
    {
        Collapse("avi", SourceTextBox.Text, SourceTextBox.Text, SearchOption.AllDirectories);
        Collapse("mkv", SourceTextBox.Text, SourceTextBox.Text, SearchOption.AllDirectories);
        Collapse("mp4", SourceTextBox.Text, SourceTextBox.Text, SearchOption.AllDirectories);
        Collapse("srt", SourceTextBox.Text, SourceTextBox.Text, SearchOption.AllDirectories);
        System.Windows.MessageBox.Show("CollapseFlatten complete.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /*private void CollapseToDestButton_Click(object sender, RoutedEventArgs e)
    {
        Collapse("avi", SourceTextBox.Text, DestTextBox.Text, SearchOption.TopDirectoryOnly);
        Collapse("mkv", SourceTextBox.Text, DestTextBox.Text, SearchOption.TopDirectoryOnly);
        Collapse("mp4", SourceTextBox.Text, DestTextBox.Text, SearchOption.TopDirectoryOnly);
        Collapse("srt", SourceTextBox.Text, DestTextBox.Text, SearchOption.TopDirectoryOnly);
        System.Windows.MessageBox.Show("CollapseToDest complete.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }*/

    private void Collapse(string extension, string source, string destination, SearchOption searchOption)
    {
        var info = new DirectoryInfo(source);
        foreach (var file in info.GetFiles("*." + extension, searchOption))
        {
            try
            {
                File.Move(file.FullName, Path.Combine(destination, file.Name));
            }
            catch (Exception)
            {
                // Ignore errors
            }
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