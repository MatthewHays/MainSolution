using System.Runtime.InteropServices;
using System.Windows.Threading;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace GuitarPractice.Wpf;

/// <summary>
/// Drives the beat loop. Fires <see cref="Beat"/> on every tick and
/// <see cref="ChordChanged"/> whenever the current chord advances.
/// All events are raised on the UI thread via the supplied <see cref="Dispatcher"/>.
/// Metronome ticks are played through the audio device via NAudio.
/// </summary>
public sealed class MetronomeEngine : IDisposable
{
    private readonly Dispatcher _dispatcher;
    private DispatcherTimer? _timer;

    private string[] _chords = [];
    private int _currentChordIndex;
    private int _beatsPerChord;
    private int _beatInChord;
    private bool _disposed;

    private WaveOutEvent? _tickOut;
    private float _volume = 0.8f;

    /// <summary>Master volume 0.0–1.0 applied to all tick sounds.</summary>
    public float Volume
    {
        get => _volume;
        set => _volume = Math.Clamp(value, 0f, 1f);
    }

    public int Bpm { get; private set; } = 80;

    /// <summary>Fired on every beat. Arg = beat number within the current chord (1-based).</summary>
    public event Action<int>? Beat;

    /// <summary>Fired when the current chord changes. Arg = new chord index.</summary>
    public event Action<int>? ChordChanged;

    public bool IsRunning => _timer?.IsEnabled == true;

    public MetronomeEngine(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public void SetChords(string[] chords, int beatsPerChord = 4)
    {
        _chords = chords;
        _beatsPerChord = Math.Max(1, beatsPerChord);
        _currentChordIndex = 0;
        _beatInChord = 0;
    }

    public void SetBpm(int bpm)
    {
        Bpm = Math.Clamp(bpm, 20, 240);
        if (IsRunning)
            _timer!.Interval = BeatInterval();
    }

    public void Start()
    {
        if (_chords.Length == 0) return;

        _currentChordIndex = 0;
        _beatInChord = 0;

        _timer ??= new DispatcherTimer();
        _timer.Tick -= OnTick;
        _timer.Tick += OnTick;
        _timer.Interval = BeatInterval();
        _timer.Start();

        // Fire the initial chord immediately so synthesizer plays straight away
        ChordChanged?.Invoke(_currentChordIndex);
    }

    public void Stop()
    {
        _timer?.Stop();
        StopTick();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        // Advance beat counter; when we've completed a full bar, move to the next chord
        if (_beatInChord >= _beatsPerChord)
        {
            _beatInChord = 0;
            _currentChordIndex = (_currentChordIndex + 1) % _chords.Length;
            ChordChanged?.Invoke(_currentChordIndex);
        }

        _beatInChord++;
        bool isAccent = _beatInChord == 1;
        PlayTickSound(isAccent);
        Beat?.Invoke(_beatInChord);
    }

    private TimeSpan BeatInterval() => TimeSpan.FromMilliseconds(60_000.0 / Bpm);

    /// <summary>
    /// Plays a short click through the audio device.
    /// Accent beat (beat 1) uses a higher-pitched, sharper click.
    /// </summary>
    private void PlayTickSound(bool accent)
    {
        try
        {
            StopTick();
            var samples = GenerateClick(accent);
            var provider = new RawClickProvider(samples, 44100);
            _tickOut = new WaveOutEvent { DesiredLatency = 80 };
            _tickOut.Init(provider);
            _tickOut.Volume = _volume;
            _tickOut.Play();
        }
        catch { /* non-critical — don't crash the metronome */ }
    }

    private static float[] GenerateClick(bool accent)
    {
        // Kick drum synthesis:
        //   - A sine wave that sweeps rapidly from a high start frequency down to a low body frequency
        //     (the characteristic "thump" of a kick)
        //   - Mixed with a short burst of low-passed noise for the transient "punch"
        //   - Accent beat is louder and has a slightly higher start pitch

        const int sampleRate  = 44100;
        const double duration = 0.18;   // 180 ms total — long enough for the tail
        int count = (int)(sampleRate * duration);
        var buf   = new float[count];

        double startHz  = accent ? 180.0 : 140.0;  // pitch sweep start
        double endHz    = 40.0;                     // pitch sweep end (body thump)
        double sweepDecay = 25.0;                   // how fast the pitch drops
        double ampDecay   = 18.0;                   // overall amplitude decay rate
        double noiseDecay = 60.0;                   // noise punch decays much faster
        double noiseGain  = accent ? 0.35 : 0.25;
        double sineGain   = accent ? 0.90 : 0.75;

        var rng   = new Random(42);           // fixed seed for consistent tone
        double phase = 0.0;

        for (int i = 0; i < count; i++)
        {
            double t = (double)i / sampleRate;

            // Exponentially falling pitch sweep
            double hz  = endHz + (startHz - endHz) * Math.Exp(-sweepDecay * t);
            double amp = Math.Exp(-ampDecay * t);

            // Advance phase by current instantaneous frequency
            phase += 2.0 * Math.PI * hz / sampleRate;

            double sine  = Math.Sin(phase) * amp * sineGain;

            // Low-frequency noise transient (the attack "click/punch")
            double noise = (rng.NextDouble() * 2.0 - 1.0)
                           * Math.Exp(-noiseDecay * t)
                           * noiseGain;

            buf[i] = (float)(sine + noise);
        }

        return buf;
    }

    private void StopTick()
    {
        try { _tickOut?.Stop(); _tickOut?.Dispose(); }
        catch { /* ignore */ }
        _tickOut = null;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer?.Stop();
        _timer = null;
        StopTick();
    }

    private sealed class RawClickProvider : ISampleProvider
    {
        private readonly float[] _samples;
        private int _pos;
        public WaveFormat WaveFormat { get; }

        public RawClickProvider(float[] samples, int sampleRate)
        {
            _samples = samples;
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int available = Math.Min(count, _samples.Length - _pos);
            for (int i = 0; i < available; i++)
                buffer[offset + i] = _samples[_pos + i];
            _pos += available;
            return available;
        }
    }
}
