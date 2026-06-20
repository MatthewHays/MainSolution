using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GuitarPractice.Wpf;

public partial class MainWindow : Window
{
    private readonly ChordLookupService _lookupService = new();
    private readonly MetronomeEngine _metronome = new(System.Windows.Threading.Dispatcher.CurrentDispatcher);
    private readonly ChordSynthesizer _synth = new();

    // Currently displayed chord borders — kept for highlight toggling
    private readonly List<Border> _chordBorders = [];
    private int _currentChordIndex = -1;
    private int _beatsPerChord = 4;

    // Brushes
    private static readonly SolidColorBrush BrushInactive   = new(Color.FromRgb(0x1F, 0x2A, 0x44));
    private static readonly SolidColorBrush BrushActive     = new(Color.FromRgb(0x2E, 0x7D, 0x32));
    private static readonly SolidColorBrush BrushBeatOn     = new(Color.FromRgb(0xFF, 0xCA, 0x28));
    private static readonly SolidColorBrush BrushBeatOff    = new(Color.FromRgb(0x37, 0x47, 0x4F));
    private static readonly SolidColorBrush BrushAccentBeat = new(Color.FromRgb(0xFF, 0x57, 0x22));

    public MainWindow()
    {
        _metronome.Beat         += OnBeat;
        _metronome.ChordChanged += OnChordChanged;
        InitializeComponent();
    }

    // ── Search ──────────────────────────────────────────────────────────────

    private void SongSearchBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            _ = LookupChordsAsync();
    }

    private void LookupButton_Click(object sender, RoutedEventArgs e)
        => _ = LookupChordsAsync();

    private async Task LookupChordsAsync()
    {
        var query = SongSearchBox.Text.Trim();
        if (string.IsNullOrEmpty(query)) return;

        StopPlayback();
        SetStatus("Searching…", isError: false, isItalic: true);
        LookupButton.IsEnabled = false;

        List<SongSearchResult> searchResults;
        try
        {
            searchResults = await _lookupService.SearchSongsAsync(query);
        }
        catch
        {
            searchResults = [];
        }

        LookupButton.IsEnabled = true;

        if (searchResults.Count == 0)
        {
            SetStatus($"No songs found for \"{query}\".", isError: true);
            ClearChords();
            return;
        }

        // If there's only one result, skip the picker
        SongSearchResult chosen;
        if (searchResults.Count == 1)
        {
            chosen = searchResults[0];
        }
        else
        {
            var picker = new SongPickerWindow(searchResults, this);
            if (picker.ShowDialog() != true || picker.SelectedResult == null)
            {
                SetStatus("No song selected.", isError: false, isItalic: true);
                return;
            }
            chosen = picker.SelectedResult;
        }

        SetStatus($"Loading chords for {chosen.DisplayTitle}…", isError: false, isItalic: true);
        LookupButton.IsEnabled = false;

        var result = await _lookupService.LookupFromResultAsync(chosen);

        LookupButton.IsEnabled = true;

        if (result.IsError)
        {
            SetStatus(result.ErrorMessage, isError: true);
            ClearChords();
            return;
        }

        var offlineBadge = result.IsOffline ? " [offline]" : "";
        var bpmBadge     = result.Bpm > 0   ? $"  •  {result.Bpm} BPM" : "";
        SetStatus($"{result.Title}{offlineBadge}{bpmBadge}", isError: false, isItalic: false);

        if (result.Bpm > 0)
        {
            var clampedBpm = Math.Clamp(result.Bpm, (int)BpmSlider.Minimum, (int)BpmSlider.Maximum);
            BpmSlider.Value = clampedBpm;
        }

        LoadChords(result.Chords);
    }

    // ── Chord display ────────────────────────────────────────────────────────

    private void LoadChords(string[] chords)
    {
        ClearChords();
        if (chords.Length == 0) return;

        foreach (var chord in chords)
        {
            var text = new TextBlock
            {
                Text = chord,
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var border = new Border
            {
                Background = BrushInactive,
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(5, 4, 5, 4),
                Padding = new Thickness(14, 8, 14, 8),
                MinWidth = 64,
                Child = text
            };

            _chordBorders.Add(border);
            ChordsPanel.Children.Add(border);
        }

        NoChordLabel.Visibility = Visibility.Collapsed;
        ChordsPanel.Visibility  = Visibility.Visible;

        _metronome.SetChords(chords, _beatsPerChord);

        PlayButton.IsEnabled = true;
        StopButton.IsEnabled = false;
    }

    private void ClearChords()
    {
        _chordBorders.Clear();
        ChordsPanel.Children.Clear();
        ChordsPanel.Visibility  = Visibility.Collapsed;
        NoChordLabel.Visibility = Visibility.Visible;
        _currentChordIndex = -1;
        PlayButton.IsEnabled = false;
        StopButton.IsEnabled = false;
    }

    private void HighlightChord(int index)
    {
        if (_currentChordIndex >= 0 && _currentChordIndex < _chordBorders.Count)
            _chordBorders[_currentChordIndex].Background = BrushInactive;

        _currentChordIndex = index;

        if (index >= 0 && index < _chordBorders.Count)
        {
            _chordBorders[index].Background = BrushActive;
            // Scroll into view if the panel overflows
            _chordBorders[index].BringIntoView();
        }
    }

    // ── Metronome callbacks ──────────────────────────────────────────────────

    private void OnBeat(int beatNumber)
    {
        // Flash the beat indicator
        bool isAccent = beatNumber == 1;
        BeatIndicator.Background = isAccent ? BrushAccentBeat : BrushBeatOn;

        // Fade it back after 80 ms
        var animation = new ColorAnimation(
            isAccent ? Color.FromRgb(0xFF, 0x57, 0x22) : Color.FromRgb(0xFF, 0xCA, 0x28),
            Color.FromRgb(0x37, 0x47, 0x4F),
            new Duration(TimeSpan.FromMilliseconds(200)));

        BeatIndicator.Background = new SolidColorBrush(
            isAccent ? Color.FromRgb(0xFF, 0x57, 0x22) : Color.FromRgb(0xFF, 0xCA, 0x28));
        ((SolidColorBrush)BeatIndicator.Background).BeginAnimation(
            SolidColorBrush.ColorProperty, animation);
    }

    private void OnChordChanged(int index)
    {
        HighlightChord(index);

        // Play chord audio on a background thread so it doesn't block the UI
        if (index >= 0 && index < _chordBorders.Count)
        {
            var chordName = ((TextBlock)_chordBorders[index].Child).Text;
            // Bar duration = one beat * beats-per-chord
            double barSeconds = (60.0 / _metronome.Bpm) * _beatsPerChord;
            Task.Run(() => _synth.PlayChord(chordName, barSeconds));
        }
    }

    // ── Transport ────────────────────────────────────────────────────────────

    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        _metronome.SetChords(
            [.. _chordBorders.Select(b => ((TextBlock)b.Child).Text)],
            _beatsPerChord);
        _metronome.Start();
        PlayButton.IsEnabled = false;
        StopButton.IsEnabled = true;
    }

    private void StopButton_Click(object sender, RoutedEventArgs e) => StopPlayback();

    private void StopPlayback()
    {
        _metronome.Stop();
        ResetHighlight();
        if (_chordBorders.Count > 0)
        {
            PlayButton.IsEnabled = true;
            StopButton.IsEnabled = false;
        }
        BeatIndicator.Background = BrushBeatOff;
    }

    private void ResetHighlight()
    {
        foreach (var b in _chordBorders)
            b.Background = BrushInactive;
        _currentChordIndex = -1;
    }

    // ── BPM / Beats per chord ────────────────────────────────────────────────

    private void BpmSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        int bpm = (int)e.NewValue;
        _metronome.SetBpm(bpm);
        if (BpmLabel != null)
            BpmLabel.Text = $"{bpm} BPM";
    }

    private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        float volume = (float)(e.NewValue / 100.0);
        _metronome.Volume = volume;
        _synth.Volume     = volume;
        if (VolumeLabel != null)
            VolumeLabel.Text = $"{(int)e.NewValue} %";
    }

    private void BeatsPerChordCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BeatsPerChordCombo?.SelectedItem is ComboBoxItem item
            && int.TryParse(item.Content?.ToString(), out var beats))
        {
            _beatsPerChord = beats;

            // Always push the new value into the engine regardless of running state.
            // If already running, restart cleanly so the beat counter resets.
            var chords = _chordBorders.Select(b => ((TextBlock)b.Child).Text).ToArray();
            bool wasRunning = _metronome.IsRunning;
            if (wasRunning) _metronome.Stop();
            _metronome.SetChords(chords, _beatsPerChord);
            if (wasRunning) _metronome.Start();
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void SetStatus(string message, bool isError, bool isItalic = false)
    {
        SongTitleLabel.Text       = message;
        SongTitleLabel.Foreground = isError
            ? new SolidColorBrush(Color.FromRgb(0xEF, 0x53, 0x50))
            : new SolidColorBrush(Color.FromRgb(0x90, 0xA4, 0xAE));
        SongTitleLabel.FontStyle  = isItalic ? FontStyles.Italic : FontStyles.Normal;
    }

    protected override void OnClosed(EventArgs e)
    {
        _metronome.Dispose();
        _synth.Dispose();
        base.OnClosed(e);
    }
}
