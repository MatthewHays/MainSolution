using NAudio.Wave;

namespace GuitarPractice.Wpf;

/// <summary>
/// Synthesizes sustained drone chords using additive synthesis.
/// Each note is built from a fundamental + harmonics with slight detuning
/// between oscillators, producing a thick, organ-like pad sound that holds
/// for the full bar duration then fades out.
/// </summary>
public sealed class ChordSynthesizer : IDisposable
{
    private WaveOutEvent? _waveOut;
    private bool _disposed;
    private float _volume = 0.8f;

    /// <summary>Master volume 0.0–1.0. Applied to all subsequent PlayChord calls.</summary>
    public float Volume
    {
        get => _volume;
        set
        {
            _volume = Math.Clamp(value, 0f, 1f);
            if (_waveOut != null) _waveOut.Volume = _volume;
        }
    }

    private const int SampleRate = 44100;

    // Attack / release times in seconds
    private const double AttackTime  = 0.04;
    private const double ReleaseTime = 0.35;

    // --- Note frequency table (MIDI → Hz) ---
    private static double MidiToHz(int midi) =>
        440.0 * Math.Pow(2.0, (midi - 69) / 12.0);

    // Note name → MIDI number (guitar range)
    private static readonly Dictionary<string, int> NoteToMidi = new(StringComparer.OrdinalIgnoreCase)
    {
        ["C"]  = 48, ["C#"] = 49, ["Db"] = 49,
        ["D"]  = 50, ["D#"] = 51, ["Eb"] = 51,
        ["E"]  = 52, ["F"]  = 53, ["F#"] = 54, ["Gb"] = 54,
        ["G"]  = 55, ["G#"] = 56, ["Ab"] = 56,
        ["A"]  = 45, ["A#"] = 46, ["Bb"] = 46,
        ["B"]  = 47,
    };

    // Chord quality → semitone intervals above root
    private static readonly Dictionary<string, int[]> ChordIntervals = new(StringComparer.OrdinalIgnoreCase)
    {
        [""]     = [0, 4, 7],
        ["m"]    = [0, 3, 7],
        ["dim"]  = [0, 3, 6],
        ["aug"]  = [0, 4, 8],
        ["7"]    = [0, 4, 7, 10],
        ["maj7"] = [0, 4, 7, 11],
        ["m7"]   = [0, 3, 7, 10],
        ["dim7"] = [0, 3, 6, 9],
        ["m7b5"] = [0, 3, 6, 10],
        ["sus2"] = [0, 2, 7],
        ["sus4"] = [0, 5, 7],
        ["add9"] = [0, 4, 7, 14],
        ["9"]    = [0, 4, 7, 10, 14],
        ["maj9"] = [0, 4, 7, 11, 14],
        ["m9"]   = [0, 3, 7, 10, 14],
        ["5"]    = [0, 7],
        ["6"]    = [0, 4, 7, 9],
        ["m6"]   = [0, 3, 7, 9],
        ["11"]   = [0, 4, 7, 10, 14, 17],
        ["13"]   = [0, 4, 7, 10, 14, 17, 21],
    };

    // Guitar voicings (MIDI notes)
    private static readonly Dictionary<string, int[]> FullVoicings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["G"]      = [43, 47, 55, 59, 62, 67],
        ["C"]      = [48, 52, 55, 60, 64],
        ["D"]      = [50, 57, 62, 66],
        ["E"]      = [40, 47, 52, 56, 59, 64],
        ["Em"]     = [40, 47, 52, 55, 59, 64],
        ["Am"]     = [45, 52, 57, 60, 64],
        ["A"]      = [45, 52, 57, 61, 64],
        ["F"]      = [41, 48, 53, 57, 60, 65],
        ["Bm"]     = [47, 54, 59, 62, 66],
        ["B"]      = [47, 54, 59, 63, 66],
        ["Em7"]    = [40, 47, 52, 55, 59, 62],
        ["Dm"]     = [50, 57, 62, 65],
        ["Fm"]     = [41, 48, 53, 56, 60, 65],
        ["Cm"]     = [48, 55, 60, 63, 67],
        ["Gm"]     = [43, 50, 55, 58, 62, 67],
        ["A7"]     = [45, 52, 55, 59, 64],
        ["E7"]     = [40, 47, 52, 56, 59, 62],
        ["B7"]     = [47, 54, 57, 59, 63, 66],
        ["D7"]     = [50, 57, 60, 62, 66],
        ["G7"]     = [43, 47, 53, 59, 62, 65],
        ["C7"]     = [48, 52, 58, 60, 64],
        ["F#"]     = [42, 49, 54, 58, 61, 66],
        ["F#m"]    = [42, 49, 54, 57, 61, 66],
        ["Bb"]     = [46, 53, 58, 62, 65, 70],
        ["Eb"]     = [51, 58, 63, 67, 70],
        ["Ab"]     = [44, 51, 56, 60, 63, 68],
        ["Db"]     = [49, 56, 61, 65, 68],
        ["G/B"]    = [47, 55, 59, 62, 67],
        ["Cadd9"]  = [48, 55, 60, 62, 64, 67],
        ["Am7"]    = [45, 52, 55, 57, 60, 64],
        ["Dsus4"]  = [50, 57, 62, 67],
        ["A7sus4"] = [45, 52, 57, 62, 64],
        ["Fsus2"]  = [41, 48, 53, 55, 60],
    };

    /// <summary>
    /// Plays a sustained drone chord for <paramref name="durationSeconds"/>.
    /// Call this on a background thread — it returns immediately while audio plays.
    /// </summary>
    public void PlayChord(string chordName, double durationSeconds)
    {
        if (string.IsNullOrWhiteSpace(chordName)) return;

        var midiNotes = ResolveChordNotes(chordName);
        if (midiNotes.Length == 0) return;

        StopCurrent();

        var samples = SynthesizeDrone(midiNotes, durationSeconds);
        var provider = new RawSampleProvider(samples, SampleRate);

        _waveOut = new WaveOutEvent { DesiredLatency = 100 };
        _waveOut.Init(provider);
        _waveOut.Volume = _volume;
        _waveOut.Play();
    }

    private void StopCurrent()
    {
        try { _waveOut?.Stop(); _waveOut?.Dispose(); }
        catch { /* ignore */ }
        _waveOut = null;
    }

    // ── Note resolution ──────────────────────────────────────────────────────

    private static int[] ResolveChordNotes(string chordName)
    {
        if (FullVoicings.TryGetValue(chordName, out var v)) return v;

        var slashIdx = chordName.IndexOf('/');
        if (slashIdx > 0 && FullVoicings.TryGetValue(chordName[..slashIdx], out v)) return v;

        return BuildFromIntervals(chordName);
    }

    private static int[] BuildFromIntervals(string chordName)
    {
        var name = chordName.Contains('/') ? chordName[..chordName.IndexOf('/')] : chordName;
        if (name.Length == 0) return [];

        string root, quality;
        if (name.Length >= 2 && (name[1] == '#' || name[1] == 'b'))
        { root = name[..2]; quality = name[2..]; }
        else
        { root = name[..1]; quality = name[1..]; }

        if (!NoteToMidi.TryGetValue(root, out var rootMidi)) return [];

        int[]? intervals = null;
        int bestLen = -1;
        foreach (var (suffix, ints) in ChordIntervals)
        {
            if (quality.Equals(suffix, StringComparison.OrdinalIgnoreCase) && suffix.Length > bestLen)
            { intervals = ints; bestLen = suffix.Length; }
        }

        intervals ??= ChordIntervals[""];
        return [.. intervals.Select(i => rootMidi + i)];
    }

    // ── Drone synthesis ──────────────────────────────────────────────────────

    /// <summary>
    /// Builds a sustained pad/drone by summing additive oscillators for each note.
    /// Each note gets:
    ///   - A fundamental sine wave
    ///   - A slightly detuned second oscillator (+4 cents) for chorus width
    ///   - A 2nd harmonic at half amplitude
    ///   - A 3rd harmonic at quarter amplitude
    /// The whole signal has an ADSR-style attack and a smooth release tail.
    /// </summary>
    private static float[] SynthesizeDrone(int[] midiNotes, double durationSeconds)
    {
        // Add a small tail beyond the bar so the release doesn't clip hard
        double totalDuration = durationSeconds + ReleaseTime;
        int totalSamples = (int)(SampleRate * totalDuration);
        var mix = new double[totalSamples];

        double gainPerNote = 0.75 / midiNotes.Length;

        foreach (var midi in midiNotes)
        {
            double hz = MidiToHz(midi);
            AddDroneNote(mix, hz, gainPerNote, durationSeconds, totalSamples);
        }

        // Convert to float and soft-clip
        var output = new float[totalSamples];
        for (int i = 0; i < totalSamples; i++)
            output[i] = (float)Math.Tanh(mix[i]); // tanh soft-clips and adds warmth

        return output;
    }

    private static void AddDroneNote(double[] mix, double hz, double gain,
                                     double sustainUntil, int totalSamples)
    {
        // Detune a second oscillator by +4 cents for a chorus-like width
        double hzDetuned = hz * Math.Pow(2.0, 4.0 / 1200.0);

        int attackSamples  = (int)(SampleRate * AttackTime);
        int releaseSamples = (int)(SampleRate * ReleaseTime);
        int sustainEndSample = (int)(SampleRate * sustainUntil);

        for (int i = 0; i < totalSamples; i++)
        {
            double t = (double)i / SampleRate;

            // Envelope
            double env;
            if (i < attackSamples)
            {
                // Smooth attack: squared curve for a gentle swell
                double p = (double)i / attackSamples;
                env = p * p;
            }
            else if (i < sustainEndSample)
            {
                env = 1.0;
            }
            else
            {
                // Exponential release
                int releaseI = i - sustainEndSample;
                double p = (double)releaseI / releaseSamples;
                env = Math.Exp(-p * 5.0);
            }

            // Additive synthesis: fundamental + detuned + 2nd + 3rd harmonic
            double sample =
                  Math.Sin(2.0 * Math.PI * hz        * t) * 1.00   // fundamental
                + Math.Sin(2.0 * Math.PI * hzDetuned * t) * 0.70   // detuned (chorus)
                + Math.Sin(2.0 * Math.PI * hz * 2    * t) * 0.40   // 2nd harmonic
                + Math.Sin(2.0 * Math.PI * hz * 3    * t) * 0.15;  // 3rd harmonic

            mix[i] += sample * env * gain;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        StopCurrent();
    }

    // ── Raw PCM provider ─────────────────────────────────────────────────────

    private sealed class RawSampleProvider : ISampleProvider
    {
        private readonly float[] _samples;
        private int _position;
        public WaveFormat WaveFormat { get; }

        public RawSampleProvider(float[] samples, int sampleRate)
        {
            _samples = samples;
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int available = Math.Min(count, _samples.Length - _position);
            for (int i = 0; i < available; i++)
                buffer[offset + i] = _samples[_position + i];
            _position += available;
            return available;
        }
    }
}
