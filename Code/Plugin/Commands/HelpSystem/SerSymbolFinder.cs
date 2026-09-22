using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SER.Code.Plugin.Commands.HelpSystem;

public enum SerSymbolKind { Method, Event, EventVariable, Enum, Property, Keyword, Variable, Flag }

/// <summary>Canonical metadata only. Discovery never reads a live value or executes a script.</summary>
public sealed record SerSymbol(string Id, SerSymbolKind Kind, string Name, string Description,
    string HelpQuery, string? Owner = null, string? RequiresIntegration = null, bool Available = true,
    IReadOnlyList<string>? Aliases = null);

public sealed record SerSearchCandidate(SerSymbol Symbol, int LexicalScore, bool Exact);
/// <summary>Relevance is finite and in [0,1]. IDs must refer to the supplied candidates.</summary>
public sealed record SerRelevanceScore(string SymbolId, double Relevance);
public interface ISerRelevanceScorer
{
    Task<IReadOnlyList<SerRelevanceScore>> ScoreAsync(string query,
        IReadOnlyList<SerSearchCandidate> candidates, CancellationToken cancellationToken);
}

public sealed record SerFindResult(IReadOnlyList<SerSearchCandidate> Results, bool UsedScorer,
    string? FallbackReason, int CandidateCount);

/// <summary>Immutable catalogue snapshot; callers may rebuild it when registries change.</summary>
public sealed class SerSymbolFinder
{
    private readonly SerSymbol[] _symbols;
    public SerSymbolFinder(IEnumerable<SerSymbol> symbols)
    {
        _symbols = symbols.OrderBy(s => s.Id, StringComparer.Ordinal).ToArray();
        if (_symbols.Select(s => s.Id).Distinct(StringComparer.Ordinal).Count() != _symbols.Length)
            throw new ArgumentException("Symbol identities must be unique.", nameof(symbols));
    }

    public async Task<SerFindResult> FindAsync(string query, ISerRelevanceScorer? scorer,
        int limit = 10, CancellationToken cancellationToken = default, TimeSpan? scorerTimeout = null)
    {
        cancellationToken.ThrowIfCancellationRequested();
        query = (query ?? string.Empty).Trim();
        if (query.Length > 300) throw new ArgumentException("Search must be 300 characters or fewer.", nameof(query));
        limit = Math.Max(1, Math.Min(50, limit));
        if (query.Length == 0) return new SerFindResult(Array.Empty<SerSearchCandidate>(), false, null, 0);
        var ranked = _symbols.Select(s => { cancellationToken.ThrowIfCancellationRequested(); return Rank(s, query); }).OrderByDescending(c => c.LexicalScore)
            .ThenBy(c => c.Symbol.Id, StringComparer.Ordinal).ToArray();
        var local = ranked.Where(c => c.LexicalScore > 0).Take(limit).ToArray();
        if (scorer is null) return new SerFindResult(local, false, null, ranked.Length);

        // Natural-language requests often have no strong lexical match. Include the whole
        // snapshot in that case, so semantic scoring can discover different terminology.
        var candidates = ranked.FirstOrDefault()?.LexicalScore >= 700
            ? ranked.Take(Math.Max(128, limit)).ToArray() : ranked;
        var timeout = scorerTimeout ?? TimeSpan.FromSeconds(12);
        if (timeout <= TimeSpan.Zero || timeout > TimeSpan.FromMinutes(1))
            throw new ArgumentOutOfRangeException(nameof(scorerTimeout));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linked.CancelAfter(timeout);
        try
        {
            // Also bound scorers that ignore cancellation or block before returning their task.
            var scoring = Task.Run(() => scorer.ScoreAsync(query, Array.AsReadOnly(candidates), linked.Token), linked.Token);
            var deadline = Task.Delay(timeout, cancellationToken);
            if (await Task.WhenAny(scoring, deadline).ConfigureAwait(false) != scoring)
            {
                linked.Cancel();
                _ = scoring.ContinueWith(t => { _ = t.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
                cancellationToken.ThrowIfCancellationRequested();
                return new SerFindResult(local, false, "Scoring timed out; showing local matches.", candidates.Length);
            }
            var scores = await scoring.ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            var ids = new HashSet<string>(candidates.Select(c => c.Symbol.Id), StringComparer.Ordinal);
            if (scores is null || scores.Count != candidates.Length ||
                scores.Any(s => s is null || !ids.Contains(s.SymbolId) || double.IsNaN(s.Relevance) ||
                    double.IsInfinity(s.Relevance) || s.Relevance < 0 || s.Relevance > 1) ||
                scores.Select(s => s.SymbolId).Distinct(StringComparer.Ordinal).Count() != scores.Count)
                return new SerFindResult(local, false, "Scoring returned invalid results; showing local matches.", candidates.Length);
            var byId = scores.ToDictionary(s => s.SymbolId, s => s.Relevance, StringComparer.Ordinal);
            return new SerFindResult(candidates.OrderByDescending(c => c.Exact)
                .ThenByDescending(c => byId[c.Symbol.Id]).ThenByDescending(c => c.LexicalScore)
                .ThenBy(c => c.Symbol.Id, StringComparer.Ordinal).Take(limit).ToArray(), true, null, candidates.Length);
        }
        catch (Exception)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new SerFindResult(local, false, "Scoring unavailable; showing local matches.", candidates.Length);
        }
    }

    private static SerSearchCandidate Rank(SerSymbol symbol, string query)
    {
        var names = new[] { symbol.Name }.Concat(symbol.Aliases ?? Array.Empty<string>()).ToArray();
        var exact = names.Any(n => n.Equals(query, StringComparison.OrdinalIgnoreCase));
        var words = Words(query);
        var nameWords = Words(string.Join(" ", names));
        var allWords = Words(string.Join(" ", names) + " " + symbol.Description + " " + symbol.Owner);
        int score = exact ? 1000 : names.Any(n => n.StartsWith(query, StringComparison.OrdinalIgnoreCase)) ? 800
            : words.Length > 0 && words.All(w => nameWords.Contains(w)) ? 700
            : names.Any(n => n.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) ? 600
            : words.Length > 0 && words.All(w => allWords.Contains(w)) ? 500
            : Math.Min(200, words.Count(w => allWords.Contains(w)) * 30);
        if (score < 300 && query.Length >= 3)
        {
            var distance = names.Min(n => Distance(n.ToLowerInvariant(), query.ToLowerInvariant()));
            if (distance <= Math.Min(3, query.Length / 3)) score = Math.Max(score, 300 - distance * 40);
        }
        return new SerSearchCandidate(symbol, score, exact);
    }

    private static string[] Words(string text) => Regex.Split(
        Regex.Replace(text, "([a-z])([A-Z])", "$1 $2").ToLowerInvariant(), "[^a-z0-9]+").Where(w => w.Length > 0).Distinct().ToArray();

    private static int Distance(string a, string b)
    {
        if (Math.Abs(a.Length - b.Length) > 3) return 4;
        var row = Enumerable.Range(0, b.Length + 1).ToArray();
        for (var i = 1; i <= a.Length; i++)
        {
            var previous = row[0]; row[0] = i;
            for (var j = 1; j <= b.Length; j++)
            {
                var saved = row[j];
                row[j] = Math.Min(Math.Min(row[j] + 1, row[j - 1] + 1), previous + (a[i - 1] == b[j - 1] ? 0 : 1));
                previous = saved;
            }
        }
        return row[b.Length];
    }
}
