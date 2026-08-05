using MEC;
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
public class WaitUntilKeyword : YieldingContext, IKeywordContext
{
    protected readonly List<BaseToken> Tokens = [];

    private NumericExpressionResolver.CompiledExpression _expression = null!;

    public override string FriendlyName => $"'{KeywordName}' keyword";
    public virtual string KeywordName => "wait_until";

    public virtual string Description => "Halts execution of the script until a condition is met.";

    public virtual ContextArgument[] Arguments => [ContextArgument.Required(
        "$condition", "Expression polled until it evaluates to true.",
        "A boolean expression; it is evaluated once per frame while waiting.")];

    public virtual string Example =>
        """
        # Wait until the server is empty.
        wait_until {AmountOf @all} is 0

        # Conditions can use variables and properties too.
        wait_until {@sender -> isAlive} is false
        """;

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        Tokens.Add(token);
        return TryAddTokenRes.Continue();
    }

    public override Result VerifyCurrentState()
    {
        if (Tokens.Count == 0)
        {
            return $"The condition was not provided for the '{KeywordName}' keyword.";
        }

        if (NumericExpressionResolver.CompileExpression(Tokens.ToArray())
            .HasErrored(out var error, out var cond))
        {
            return error;
        }

        _expression = cond;
        return true;
    }

    protected override IEnumerator<float> Execute()
    {
        while (!GetConditionResult())
        {
            yield return Timing.WaitForOneFrame;
        }
    }

    private bool GetConditionResult()
    {
        if (_expression.Evaluate().HasErrored(out var error, out var objResult))
        {
            throw new ScriptRuntimeError(this, error);
        }

        if (objResult is not bool result)
        {
            throw new ScriptRuntimeError(
                this,
                $"'{KeywordName}' condition must evaluate to a boolean value, but received {objResult.FriendlyTypeName()}"
            );
        }

        return result;
    }
}
