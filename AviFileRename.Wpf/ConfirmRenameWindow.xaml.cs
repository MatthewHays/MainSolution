using System.Windows;
using System.IO;

namespace AviFileRename.Wpf;

public partial class ConfirmRenameWindow : Window
{
    public ConfirmRenameWindow()
    {
        InitializeComponent();
    }

    public string SuggestedName
    {
        get => NameTextBox.Text ?? string.Empty;
        set => NameTextBox.Text = value;
    }

    public string OriginalFileName
    {
        set => OriginalLabel.Text = "Original: " + Path.GetFileName(value);
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
