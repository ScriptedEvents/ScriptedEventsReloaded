using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem.Contexts.Control;

[UsedImplicitly]
public class ElifStatement : StatementContext, IStatementExtender, IExtendableStatement, IKeywordContext
{
    private readonly List<BaseToken> _condition = [];

    private NumericExpressionReslover.CompiledExpression _expression = null!;

    public override string FriendlyName => "'elif' statement";

    public IExtendableStatement.Signal AllowedSignals => IExtendableStatement.Signal.DidntExecute;
    public Dictionary<IExtendableStatement.Signal, StatementContext> RegisteredSignals { get; } = new();
    public string KeywordName => "elif";
    public string Description =>
        "If the statement above it didn't execute, 'elif' statement will try to execute if the provided condition is met.";
    public string[] Arguments => ["[condition]"];
    public string? Example => null;

    public IExtendableStatement.Signal Extends => IExtendableStatement.Signal.DidntExecute;

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        if (NumericExpressionReslover.IsValidForExpression(token).HasErrored(out var error))
        {
            return TryAddTokenRes.Error(error);
        }

        _condition.Add(token);
        return TryAddTokenRes.Continue();
    }

    public override OldResult VerifyCurrentState()
    {
        if (NumericExpressionReslover.CompileExpression(_condition.ToArray())
            .HasErrored(out var error, out var cond))
        {
            return error;
        }

        _expression = cond;

        return OldResult.Assert(
            _condition.Count > 0,
            "An elif statement expects to have a condition, but none was provided!"
        );
    }

    protected override IEnumerator<float> Execute()
    {
        if (_expression.Evaluate().HasErrored(out var error, out var objResult))
        {
            throw new ScriptRuntimeError(this, error);
        }

        if (objResult is not bool result)
        {
            throw new ScriptRuntimeError(this, $"An elif statement condition must evaluate to a boolean value, but received {objResult.FriendlyTypeName()}");
        }

        if (!result)
        {
            if (!RegisteredSignals.TryGetValue(IExtendableStatement.Signal.DidntExecute, out var statement))
            {
                yield break;
            }

            var coro = statement.Run();
            while (coro.MoveNext())
            {
                yield return coro.Current;
            }

            yield break;
        }

        foreach (var child in Children)
        {
            switch (child)
            {
                case StandardContext standardContext:
                    standardContext.Run();
                    break;

                case YieldingContext yieldingContext:
                    var coro = yieldingContext.Run();
                    while (coro.MoveNext()) yield return coro.Current;
                    break;

                default:
                    throw new AndrzejFuckedUpException("context is not standard nor yielding");
            }
        }
    }
}