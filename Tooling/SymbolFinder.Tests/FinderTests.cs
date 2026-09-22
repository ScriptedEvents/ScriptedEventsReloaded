using SER.Code.Plugin.Commands.HelpSystem;
using Xunit;

public sealed class FinderTests
{
    private static SerSymbol Symbol(string name, string description = "") =>
        new(name, SerSymbolKind.Method, name, description, name);
    private static SerSymbolFinder Finder(params SerSymbol[] symbols) => new(symbols);
    private sealed class Scorer(Func<IReadOnlyList<SerSearchCandidate>, CancellationToken, Task<IReadOnlyList<SerRelevanceScore>>> action) : ISerRelevanceScorer
    {
        public Task<IReadOnlyList<SerRelevanceScore>> ScoreAsync(string query, IReadOnlyList<SerSearchCandidate> candidates, CancellationToken token)
            => action(candidates, token);
    }

    [Fact]
    public async Task NullScorerRanksExactPrefixWordSubstringAndTypoInOrder()
    {
        var result = await Finder(Symbol("heal"), Symbol("healing"), Symbol("Player.Heal"),
            Symbol("xhealx"), Symbol("head")).FindAsync("heal", null);
        Assert.Equal(new[] { "heal", "healing", "Player.Heal", "xhealx", "head" }, result.Results.Select(c => c.Symbol.Name));
        Assert.False(result.UsedScorer);
    }

    [Fact]
    public async Task AliasesAreExactAndTiesAreStable()
    {
        var a = Symbol("A") with { Aliases = new[] { "alias" } };
        var b = Symbol("B") with { Aliases = new[] { "alias" } };
        Assert.Equal(new[] { "A", "B" }, (await Finder(b, a).FindAsync("ALIAS", null)).Results.Select(c => c.Symbol.Name));
    }

    [Fact]
    public async Task ScoringChangesOrderButPreservesExactAndCanonicalRecords()
    {
        var exact = Symbol("heal"); var semantic = Symbol("restore", "restore health");
        var scorer = new Scorer((c, _) => Task.FromResult<IReadOnlyList<SerRelevanceScore>>(
            c.Select(x => new SerRelevanceScore(x.Symbol.Id, x.Symbol == semantic ? 1 : 0)).ToArray()));
        var result = await Finder(semantic, exact).FindAsync("heal", scorer);
        Assert.True(result.UsedScorer);
        Assert.Same(exact, result.Results[0].Symbol);
        Assert.Same(semantic, result.Results[1].Symbol);
    }

    [Fact]
    public async Task WeakMatchesBroadenBeyondFirst128()
    {
        var symbols = Enumerable.Range(0, 300).Select(i => Symbol($"item{i:000}" )).ToArray();
        var scorer = new Scorer((c, _) => {
            Assert.Equal(300, c.Count);
            return Task.FromResult<IReadOnlyList<SerRelevanceScore>>(c.Select(x => new SerRelevanceScore(x.Symbol.Id, x.Symbol.Id == "item299" ? 1 : 0)).ToArray());
        });
        Assert.Equal("item299", (await new SerSymbolFinder(symbols).FindAsync("repair wounds", scorer, 1)).Results.Single().Symbol.Id);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("unknown")]
    [InlineData("duplicate")]
    [InlineData("nan")]
    [InlineData("range")]
    [InlineData("throw")]
    public async Task InvalidOrFailingScorerFallsBack(string mode)
    {
        var finder = Finder(Symbol("heal"), Symbol("healing"));
        var scorer = new Scorer((c, _) => {
            if (mode == "throw") throw new HttpRequestException();
            IReadOnlyList<SerRelevanceScore> scores = mode switch {
                "missing" => [],
                "unknown" => [new("evil", 1), new("healing", 0)],
                "duplicate" => [new("heal", 1), new("heal", 0)],
                "nan" => [new("heal", double.NaN), new("healing", 0)],
                _ => [new("heal", 2), new("healing", 0)]
            };
            return Task.FromResult(scores);
        });
        var expected = await finder.FindAsync("heal", null);
        var result = await finder.FindAsync("heal", scorer);
        Assert.False(result.UsedScorer); Assert.NotNull(result.FallbackReason);
        Assert.Equal(expected.Results, result.Results);
    }

    [Fact]
    public async Task TimeoutBoundsAnUncooperativeScorer()
    {
        var pending = new TaskCompletionSource<IReadOnlyList<SerRelevanceScore>>();
        var result = await Finder(Symbol("heal")).FindAsync("heal", new Scorer((_, _) => pending.Task),
            scorerTimeout: TimeSpan.FromMilliseconds(25));
        Assert.False(result.UsedScorer); Assert.Single(result.Results);
        pending.SetException(new Exception("late failure"));
    }

    [Fact]
    public async Task CallerCancellationIsNotHiddenAsFallback()
    {
        using var cancellation = new CancellationTokenSource();
        var scorer = new Scorer(async (_, token) => { cancellation.Cancel(); await Task.Delay(10000, token); return []; });
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Finder(Symbol("heal")).FindAsync("heal", scorer, cancellationToken: cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Finder(Symbol("heal")).FindAsync("heal", null, cancellationToken: cancellation.Token));
    }

    [Fact]
    public async Task ResultsAndInputAreBounded()
    {
        var finder = new SerSymbolFinder(Enumerable.Range(0, 100).Select(i => Symbol("heal" + i)));
        Assert.Equal(50, (await finder.FindAsync("heal", null, 10000)).Results.Count);
        Assert.Empty((await finder.FindAsync(" ", null)).Results);
        await Assert.ThrowsAsync<ArgumentException>(() => finder.FindAsync(new string('x', 301), null));
    }
}
