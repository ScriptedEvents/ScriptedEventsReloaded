using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.ValueTokens;

namespace SER.Code.ContextSystem.Contexts.Control;

[UsedImplicitly]
public class ChanceStatement : StatementContext, IExtendableStatement, IKeywordContext
{
    private decimal? _chance;
    private Func<OldTryGet<NumberValue>>? _chanceGetter;
    
    public override string FriendlyName => "'chance' statement";
    public IExtendableStatement.Signal AllowedSignals => IExtendableStatement.Signal.DidntExecute;
    public Dictionary<IExtendableStatement.Signal, StatementContext> RegisteredSignals { get; } = [];
    public string KeywordName => "chance";
    public string Description => "This statement will execute with the provided chance.";
    public string[] Arguments => ["[chance]"];
    public string? Example => null;

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        if (token is NumberToken { Value.Value: var value })
        {
            _chance = value;
            return TryAddTokenRes.End();
        }

        if (token.CanReturn<NumberValue>(out var func))
        {
            _chanceGetter = func;
            return TryAddTokenRes.End();
        }
        
        return TryAddTokenRes.Error($"{token} cannot be interpreted as a number.");
    }

    public override OldResult VerifyCurrentState()
    {
        return OldResult.Assert(
            _chance.HasValue || _chanceGetter != null,
            "Chance was not provided.");
    }

    protected override IEnumerator<float> Execute()
    {
        if (_chance is not { } chance)
        {
            if (_chanceGetter!.Invoke().HasErrored(out var error, out var numValue))
            {
                throw new ScriptRuntimeError(this, error);
            }

            chance = numValue.Value;
        }

        if (chance is < 0 or > 1)
        {
            throw new ScriptRuntimeError(this, $"Chance must be between 0% and 100%, got {Math.Round(chance, 2)*100}%");
        }
        
        if ((decimal)new Random().NextDouble() < chance)
        {
            using var coro = RunChildren();
            while (coro.MoveNext())
            {
                yield return coro.Current;
            }
            
            yield break;
        }
        
        if (!RegisteredSignals.TryGetValue(IExtendableStatement.Signal.DidntExecute, out var statement))
        {
            yield break;
        }

        var didntExecuteCoro = statement.Run();
        while (didntExecuteCoro.MoveNext())
        {
            yield return didntExecuteCoro.Current;
        }
    }
}