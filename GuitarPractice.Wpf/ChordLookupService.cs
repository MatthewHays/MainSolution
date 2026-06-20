using System.Net.Http;
using System.Text.Json.Nodes;

namespace GuitarPractice.Wpf;

/// <summary>
/// Looks up chord progressions and BPM for a given song title.
/// Uses Ultimate Guitar for chords and GetSongBPM for tempo,
/// with a curated offline fallback for common songs.
/// </summary>
public class ChordLookupService
{
    private static readonly HttpClient _http = new()
    {
        Timeout = TimeSpan.FromSeconds(12),
        DefaultRequestHeaders =
        {
            { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36" },
            { "Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8" },
            { "Accept-Language", "en-US,en;q=0.9" },
            { "Accept-Encoding", "identity" },
        }
    };

    // Offline fallback: chords + BPM per song
    private static readonly Dictionary<string, (string[] Chords, int Bpm)> _fallback =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // ── Classic rock ──────────────────────────────────────────────
            ["whisky in a jar"]              = (["G", "Em", "C", "D"],                          102),
            ["wonderwall"]                   = (["Em7", "G", "Dsus4", "A7sus4"],                 87),
            ["knocking on heaven's door"]    = (["G", "D", "Am", "G", "D", "C"],                72),
            ["house of the rising sun"]      = (["Am", "C", "D", "F", "Am", "C", "E"],          96),
            ["sweet home chicago"]           = (["E7", "A7", "B7"],                            120),
            ["hotel california"]             = (["Bm", "F#", "A", "E", "G", "D", "Em", "F#"],   75),
            ["wish you were here"]           = (["Em7", "G", "Em7", "G", "Em7", "A", "Em7", "A", "G"], 63),
            ["brown eyed girl"]              = (["G", "C", "G", "D"],                          148),
            ["jolene"]                       = (["Am", "C", "G", "Am"],                        120),
            ["creep"]                        = (["G", "B", "C", "Cm"],                          92),
            ["leaving on a jet plane"]       = (["G", "C", "D"],                                96),
            ["take me home country roads"]   = (["G", "Em", "C", "D"],                          82),
            ["stand by me"]                  = (["G", "Em", "C", "D"],                         121),
            ["use somebody"]                 = (["C", "Am", "C", "F"],                         136),
            ["with or without you"]          = (["D", "A", "Bm", "G"],                         113),
            ["more than words"]              = (["G", "G/B", "Cadd9", "Am7", "C", "D", "Dsus4"], 96),
            ["blackbird"]                    = (["G", "Am7", "G", "G7", "C", "A7"],             96),
            ["behind blue eyes"]             = (["Em", "G", "D", "Dsus4", "Am", "C"],           75),
            ["dust in the wind"]             = (["C", "Am", "G", "Am", "D", "Am"],              96),
            ["free fallin"]                  = (["F", "Bb", "F", "C"],                          85),
            ["smells like teen spirit"]      = (["Fm", "Bb", "Ab", "Db"],                      116),
            // ── Metallica ────────────────────────────────────────────────
            ["fade to black"]                = (["Bm", "G", "Bm", "G", "E"],                   116),
            ["nothing else matters"]         = (["Em", "D", "C", "Em", "D", "C", "Am", "D"],    69),
            ["enter sandman"]                = (["Em", "G", "Em", "F#m"],                      123),
            ["the unforgiven"]               = (["Am", "C", "G", "F", "Am"],                    76),
            ["one"]                          = (["Em", "D", "Am", "C", "G"],                    94),
            ["master of puppets"]            = (["Em", "F", "Em", "Bb", "Em"],                 212),
            ["for whom the bell tolls"]      = (["Bm", "Bm7", "G", "Bm"],                       78),
            ["wherever i may roam"]          = (["Em", "Am", "Em", "G"],                       107),
            ["sad but true"]                 = (["D5", "E5", "D5", "C5"],                        98),
            ["of wolf and man"]              = (["E5", "G5", "A5"],                            134),
            ["the memory remains"]           = (["Em", "C", "G", "D"],                         118),
            ["fuel"]                         = (["E5", "G5", "B5", "A5"],                      197),
            ["until it sleeps"]              = (["Am", "F", "C", "G"],                         120),
            ["hero of the day"]              = (["Em", "C", "G", "D"],                         105),
            // ── Other rock / metal ───────────────────────────────────────
            ["sweet child o mine"]           = (["D", "C", "G", "Am"],                         122),
            ["november rain"]                = (["C", "F", "C", "G", "Am"],                     74),
            ["paradise city"]                = (["G", "C", "F", "Bb"],                         105),
            ["welcome to the jungle"]        = (["E", "G", "A", "D"],                          126),
            ["bohemian rhapsody"]            = (["Bb", "Gm", "Cm", "F", "Bb"],                  72),
            ["highway to hell"]              = (["A", "D", "G", "D", "A"],                     116),
            ["back in black"]                = (["E", "D", "A", "E"],                          96),
            ["thunder struck"]               = (["A5", "B5", "D5"],                            133),
            ["zombie"]                       = (["Em", "C", "G", "D"],                         120),
            ["wish you were here floyd"]     = (["Em7", "G", "Em7", "G"],                        63),
            ["comfortably numb"]             = (["Bm", "A", "G", "Bm"],                         63),
            ["stairway to heaven"]           = (["Am", "Am/G", "Fmaj7", "G", "C"],              82),
            ["crazy train"]                  = (["F#m", "A", "E", "D"],                        138),
            ["mr crowley"]                   = (["Dm", "C", "Bb", "A"],                        120),
            ["paranoid"]                     = (["E5", "D5", "G5", "D5"],                      164),
            ["iron man"]                     = (["B5", "D5", "E5", "G5"],                      158),
            ["war pigs"]                     = (["Dm", "C", "Bb", "C"],                         76),
            ["smoke on the water"]           = (["G5", "Bb5", "C5"],                            112),
            ["purple haze"]                  = (["E7#9", "G", "A"],                             108),
            ["all along the watchtower"]     = (["Am", "G", "F", "G"],                         116),
            ["born to be wild"]              = (["E", "D", "E", "G"],                          145),
            ["la grange"]                    = (["A5", "C5", "D5"],                            165),
            ["sharp dressed man"]            = (["G", "F", "C", "G"],                          116),
            ["sharp dressed man zz top"]     = (["G", "F", "C", "G"],                          116),
            ["sultans of swing"]             = (["Dm", "C", "Bb", "A", "Dm"],                   96),
            ["money for nothing"]            = (["G5", "F5", "Bb5"],                           134),
            ["cocaine"]                      = (["E", "D", "E"],                               104),
            ["lay down sally"]               = (["A", "E", "D"],                               112),
            ["layla"]                        = (["Dm", "Bb", "C", "Dm"],                       113),
            ["sunshine of your love"]        = (["D", "C", "G", "F#"],                        117),
            // ── Pop / indie ──────────────────────────────────────────────
            ["let it be"]                    = (["C", "G", "Am", "F"],                          74),
            ["hey jude"]                     = (["F", "C", "Bb", "F"],                          74),
            ["yesterday"]                    = (["F", "Em7", "A7", "Dm"],                       96),
            ["here comes the sun"]           = (["A", "D", "G", "A"],                          129),
            ["come as you are"]              = (["Em", "D", "Em", "D"],                        120),
            ["heart of gold"]                = (["Em", "D", "C", "G"],                          74),
            ["wanted dead or alive"]         = (["D", "Cadd9", "G", "F"],                       78),
            ["living on a prayer"]           = (["Em", "C", "D", "Em"],                        123),
            ["livin on a prayer"]            = (["Em", "C", "D", "Em"],                        123),
            ["dont stop believin"]           = (["E", "B", "C#m", "A"],                        119),
            ["don't stop believin"]          = (["E", "B", "C#m", "A"],                        119),
            ["africa"]                       = (["Bm", "G", "D", "A", "Bm"],                   93),
            ["roxanne"]                      = (["Am", "Fmaj7", "G", "Em", "Am"],              133),
            ["every breath you take"]        = (["Ab", "Fm", "Db", "Eb", "Ab"],                112),
            ["message in a bottle"]          = (["C#m", "A", "B", "F#m"],                      152),
            ["losing my religion"]           = (["Am", "Em", "Am", "Em", "Dm"],               125),
            ["man in the mirror"]            = (["G", "Am7", "C", "D"],                        100),
            ["thriller"]                     = (["Am", "E7", "Am", "Dm"],                      118),
        };

/// <summary>
/// Returns chords and BPM for the given song title.
/// Runs chord lookup and BPM lookup concurrently when online.
/// </summary>
public async Task<ChordResult> LookupAsync(string songTitle)
{
    if (string.IsNullOrWhiteSpace(songTitle))
        return ChordResult.Error("Please enter a song title.");

    // Try online lookup (chords + BPM concurrently)
    try
    {
        var chordsTask = FetchFromUltimateGuitarAsync(songTitle);
        var bpmTask    = FetchBpmAsync(songTitle);
        await Task.WhenAll(chordsTask, bpmTask);

        var onlineChords = await chordsTask;
        var onlineBpm    = await bpmTask;

        if (onlineChords != null)
        {
            int bpm = onlineBpm ?? OfflineBpm(songTitle) ?? 80;
            return onlineChords with { Bpm = bpm };
        }
    }
    catch { /* fall through to offline */ }

    // Offline fallback
    var key = songTitle.Trim();
    if (_fallback.TryGetValue(key, out var entry))
        return new ChordResult(entry.Chords, $"Offline: {key}", true, entry.Bpm);

    // Fuzzy match
    var words = key.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    foreach (var (title, data) in _fallback)
    {
        if (words.All(w => title.Contains(w, StringComparison.OrdinalIgnoreCase)))
            return new ChordResult(data.Chords, $"Offline: {title}", true, data.Bpm);
    }

    return ChordResult.Error($"No chord progression found for \"{songTitle}\". Try a different title.");
}

/// <summary>
/// Searches Ultimate Guitar for chord tabs matching the query and returns
/// up to <paramref name="maxResults"/> candidates without fetching their chords.
/// Falls back to the offline dictionary for partial matches.
/// </summary>
public async Task<List<SongSearchResult>> SearchSongsAsync(string query, int maxResults = 20)
{
    var results = new List<SongSearchResult>();

    if (string.IsNullOrWhiteSpace(query)) return results;

    // Online search
    try
    {
        var encoded = Uri.EscapeDataString(query);
        var url = $"https://www.ultimate-guitar.com/search.php?search_type=title&value={encoded}&type=Chords";
        var html = await _http.GetStringAsync(url);

        var marker = "data-content=\"";
        var start  = html.IndexOf(marker, StringComparison.Ordinal);
        if (start >= 0)
        {
            start += marker.Length;
            var end    = html.IndexOf('"', start);
            var jsonRaw = System.Web.HttpUtility.HtmlDecode(html[start..end]);
            var root    = JsonNode.Parse(jsonRaw);
            var arr     = root?["store"]?["page"]?["data"]?["results"] as JsonArray;

            if (arr != null)
            {
                foreach (var item in arr)
                {
                    if (results.Count >= maxResults) break;

                    var type = item?["type"]?.GetValue<string>();
                    if (!string.Equals(type, "Chords", StringComparison.OrdinalIgnoreCase)) continue;

                    var tabUrl    = item?["tab_url"]?.GetValue<string>()    ?? "";
                    var songName  = item?["song_name"]?.GetValue<string>()  ?? "";
                    var artist    = item?["artist_name"]?.GetValue<string>() ?? "";
                    var rating    = item?["rating"]?.GetValue<double?>() ?? 0;
                    var votes     = item?["votes"]?.GetValue<int?>()     ?? 0;

                    if (!string.IsNullOrEmpty(tabUrl) && !string.IsNullOrEmpty(songName))
                        results.Add(new SongSearchResult(songName, artist, tabUrl, rating, votes));
                }
            }
        }
    }
    catch { /* fall through to offline */ }

    // If online returned nothing, search the offline library with scored fuzzy matching
    if (results.Count == 0)
        results.AddRange(SearchOffline(query, maxResults));

    return results;
}

/// <summary>
/// Searches the offline library using scored word matching.
/// Searching "metallica" returns all Metallica songs; "fade to black" finds that entry
/// even if the words appear in any order.
/// </summary>
private static IEnumerable<SongSearchResult> SearchOffline(string query, int max)
{
    var words = query.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
    var scored = new List<(int Score, string Title)>();

    foreach (var (title, _) in _fallback)
    {
        int score = words.Count(w => title.Contains(w, StringComparison.OrdinalIgnoreCase));
        if (score > 0)
            scored.Add((score, title));
    }

    return scored
        .OrderByDescending(x => x.Score)
        .Take(max)
        .Select(x => new SongSearchResult(x.Title, "Offline", $"offline:{x.Title}", 0, 0));
}

/// <summary>Fetches chords for a specific search result already chosen by the user.</summary>
public async Task<ChordResult> LookupFromResultAsync(SongSearchResult result)
{
    // Offline entry
    if (result.TabUrl.StartsWith("offline:", StringComparison.Ordinal))
    {
        var offlineKey = result.TabUrl["offline:".Length..];
        if (_fallback.TryGetValue(offlineKey, out var entry))
            return new ChordResult(entry.Chords, $"Offline: {result.DisplayTitle}", true, entry.Bpm);
        return ChordResult.Error("Offline entry not found.");
    }

    try
    {
        var chordsTask = ExtractChordsFromTabPageAsync(result.TabUrl);
        var bpmTask    = FetchBpmAsync(result.DisplayTitle);
        await Task.WhenAll(chordsTask, bpmTask);

        var chords = await chordsTask;
        var bpm    = await bpmTask ?? OfflineBpm(result.SongName) ?? 80;

        if (chords != null && chords.Length > 0)
            return new ChordResult(chords, result.DisplayTitle, false, bpm);
    }
    catch { }

    return ChordResult.Error($"Could not load chords for \"{result.DisplayTitle}\".");
}

    // ── BPM lookup ───────────────────────────────────────────────────────────

    /// <summary>
    /// Queries the GetSongBPM public search API.
    /// Returns null if not found or on any error.
    /// </summary>
    private static async Task<int?> FetchBpmAsync(string songTitle)
    {
        try
        {
            var encoded = Uri.EscapeDataString(songTitle);
            // GetSongBPM exposes a free search endpoint — no API key needed for basic queries
            var url = $"https://getsongbpm.com/api/search?type=song&lookup={encoded}";
            var json = await _http.GetStringAsync(url);
            var root = JsonNode.Parse(json);

            var songs = root?["search"] as JsonArray;
            if (songs == null || songs.Count == 0) return null;

            // Take the first result that has a numeric tempo
            foreach (var song in songs)
            {
                var tempoNode = song?["tempo"];
                if (tempoNode == null) continue;

                var tempoStr = tempoNode.GetValue<string>();
                if (int.TryParse(tempoStr, out var bpm) && bpm is >= 20 and <= 240)
                    return bpm;

                // Sometimes it comes as a float string like "120.0"
                if (double.TryParse(tempoStr, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out var bpmD))
                {
                    var rounded = (int)Math.Round(bpmD);
                    if (rounded is >= 20 and <= 240) return rounded;
                }
            }
        }
        catch { /* non-critical */ }

        return null;
    }

    private static int? OfflineBpm(string songTitle)
    {
        var key = songTitle.Trim();
        if (_fallback.TryGetValue(key, out var entry)) return entry.Bpm;

        var words = key.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var (title, data) in _fallback)
        {
            if (words.All(w => title.Contains(w, StringComparison.OrdinalIgnoreCase)))
                return data.Bpm;
        }

        return null;
    }

    // ── Chord lookup (Ultimate Guitar) ───────────────────────────────────────

    private async Task<ChordResult?> FetchFromUltimateGuitarAsync(string songTitle)
    {
        var encoded = Uri.EscapeDataString(songTitle);
        var url = $"https://www.ultimate-guitar.com/search.php?search_type=title&value={encoded}&type=Chords";
        var html = await _http.GetStringAsync(url);

        var marker = "data-content=\"";
        var start = html.IndexOf(marker, StringComparison.Ordinal);
        if (start < 0) return null;
        start += marker.Length;
        var end = html.IndexOf('"', start);
        if (end < 0) return null;

        var jsonRaw = System.Web.HttpUtility.HtmlDecode(html[start..end]);
        var root = JsonNode.Parse(jsonRaw);
        var results = root?["store"]?["page"]?["data"]?["results"];
        if (results is not JsonArray arr || arr.Count == 0) return null;

        foreach (var item in arr)
        {
            var type = item?["type"]?.GetValue<string>();
            if (!string.Equals(type, "Chords", StringComparison.OrdinalIgnoreCase)) continue;

            var tabUrl = item?["tab_url"]?.GetValue<string>();
            if (string.IsNullOrEmpty(tabUrl)) continue;

            var songName   = item?["song_name"]?.GetValue<string>()   ?? songTitle;
            var artistName = item?["artist_name"]?.GetValue<string>()  ?? "";

            var chords = await ExtractChordsFromTabPageAsync(tabUrl);
            if (chords != null && chords.Length > 0)
            {
                var displayTitle = string.IsNullOrEmpty(artistName)
                    ? songName : $"{songName} — {artistName}";
                // Bpm will be filled in by the caller
                return new ChordResult(chords, displayTitle, false, 0);
            }
        }

        return null;
    }

    private async Task<string[]?> ExtractChordsFromTabPageAsync(string tabUrl)
    {
        var html = await _http.GetStringAsync(tabUrl);

        var marker = "data-content=\"";
        var start = html.IndexOf(marker, StringComparison.Ordinal);
        if (start < 0) return null;
        start += marker.Length;
        var end = html.IndexOf('"', start);
        if (end < 0) return null;

        var jsonRaw = System.Web.HttpUtility.HtmlDecode(html[start..end]);
        var root    = JsonNode.Parse(jsonRaw);
        var content = root?["store"]?["page"]?["data"]?["tab_view"]?["wiki_tab"]?["content"]
                        ?.GetValue<string>();

        if (string.IsNullOrWhiteSpace(content)) return null;
        return ParseChordsFromTabContent(content);
    }

    private static string[] ParseChordsFromTabContent(string content)
    {
        var seen    = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var ordered = new List<string>();
        var regex   = new System.Text.RegularExpressions.Regex(@"\[ch\]([A-G][^\[]*?)\[/ch\]");

        foreach (System.Text.RegularExpressions.Match m in regex.Matches(content))
        {
            var chord = m.Groups[1].Value.Trim();
            if (seen.Add(chord)) ordered.Add(chord);
        }

        return ordered.Count > 0 ? [.. ordered] : [];
    }
}

public record ChordResult(string[] Chords, string Title, bool IsOffline, int Bpm)
{
    public bool   IsError      { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;

    public static ChordResult Error(string message) =>
        new([], string.Empty, false, 0) { IsError = true, ErrorMessage = message };
}

public record SongSearchResult(string SongName, string Artist, string TabUrl, double Rating, int Votes)
{
    public string DisplayTitle => string.IsNullOrEmpty(Artist) || Artist == "Offline"
        ? SongName
        : $"{SongName}  —  {Artist}";
}
