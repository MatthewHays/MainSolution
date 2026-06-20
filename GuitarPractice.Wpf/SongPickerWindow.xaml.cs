using System.Windows;
using System.Windows.Controls;

namespace GuitarPractice.Wpf;

public partial class SongPickerWindow : Window
{
    public SongSearchResult? SelectedResult { get; private set; }

    public SongPickerWindow(IEnumerable<SongSearchResult> results, Window owner)
    {
        InitializeComponent();
        Owner = owner;

        // Wrap results in a view model that adds display helpers
        ResultsList.ItemsSource = results.Select(r => new SongResultViewModel(r)).ToList();
    }

    private void ResultsList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (ResultsList.SelectedItem is SongResultViewModel vm)
        {
            SelectedResult = vm.Result;
            DialogResult = true;
        }
    }

    private void SelectButton_Click(object sender, RoutedEventArgs e)
    {
        if (ResultsList.SelectedItem is SongResultViewModel vm)
        {
            SelectedResult = vm.Result;
            DialogResult = true;
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void ResultsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectButton.IsEnabled = ResultsList.SelectedItem != null;
    }
}

/// <summary>Thin view model wrapping SongSearchResult with display-ready properties.</summary>
public class SongResultViewModel(SongSearchResult result)
{
    public SongSearchResult Result { get; } = result;
    public string DisplayTitle  => Result.DisplayTitle;
    public bool   HasRating     => Result.Rating > 0;
    public string RatingDisplay => Result.Rating.ToString("F1");
}
