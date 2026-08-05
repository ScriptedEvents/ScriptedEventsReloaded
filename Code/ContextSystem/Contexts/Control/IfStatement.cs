using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem.Contexts.Control;

[UsedImplicitly]
public class IfStatement : StatementContext, IExtendableStatement, IKeywordContext
{
    private readonly List<BaseToken> _condition = [];

    private NumericExpressionResolver.CompiledExpression _expression = null!;

    public override string FriendlyName => "'if' statement";

    public IExtendableStatement.Signal AllowedSignals => IExtendableStatement.Signal.DidntExecute;
    public Dictionary<IExtendableStatement.Signal, StatementContext> RegisteredSignals { get; } = [];
    public string KeywordName => "if";
    public string Description => "This statement will execute only if the provided condition is met.";
    public ContextArgument[] Arguments => [ContextArgument.Required(
        "$condition", "Expression that must evaluate to true for the body to run.",
        "A boolean expression; values and method calls may be grouped with braces.")];
    public string Example =>
        """
        if {@sender -> isAlive}
            Print "You are still alive."
        end

        if {AmountOf @all} > 0
            Print "There is at least one player online."
        end
        """;

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        if (NumericExpressionResolver.IsValidForExpression(token).HasErrored(out var error))
        {
            return TryAddTokenRes.Error(error);
        }

        _condition.Add(token);
        return TryAddTokenRes.Continue();
    }

    public override Result VerifyCurrentState()
    {
        if (NumericExpressionResolver.CompileExpression(_condition.ToArray())
            .HasErrored(out var error, out var cond))
        {
            return error;
        }

        _expression = cond;

        return _condition.Count > 0
            ? true
            : "An if statement expects to have a condition, but none was provided!";
    }

    protected override IEnumerator<float> Execute()
    {
        if (_expression.Evaluate().HasErrored(out var error, out var objResult))
        {
            throw new ScriptRuntimeError(this, error);
        }

        if (objResult is not bool result)
        {
            throw new ScriptRuntimeError(this, $"An if statement condition must evaluate to a boolean value, but received {objResult.FriendlyTypeName()}");
        }

        if (!result)
        {
            if (!RegisteredSignals.TryGetValue(IExtendableStatement.Signal.DidntExecute, out var statement))
            {
                yield break;
            }

            using var didntExecuteCoro = statement.Run();
            while (didntExecuteCoro.MoveNext())
            {
                yield return didntExecuteCoro.Current;
            }

            yield break;
        }

        using var coro = RunChildren();
        while (coro.MoveNext())
        {
            yield return coro.Current;
        }
    }
}
