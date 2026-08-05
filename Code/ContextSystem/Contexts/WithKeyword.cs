using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;

namespace SER.Code.ContextSystem.Contexts;

[UsedImplicitly]
public class WithKeyword : StandardContext, IKeywordContext, INotRunningContext, IRequirePreviousStatementContext
{
    private readonly List<VariableToken> _variables = [];
    private Safe<IAcceptOptionalVariableDefinitionsContext> _receiver;

    public override string FriendlyName => "'with' keyword";

    public string KeywordName => "with";

    public string Description =>
        "This keyword is designed to provide a variable or a collection of variables to a statement.";

    public ContextArgument[] Arguments => [ContextArgument.Variadic(
        "[variables...]", "Defines temporary variables for the preceding statement.",
        "One or more variable names compatible with that statement.")];

    public string Example =>
        """
        over @all with @plr
            Print {@plr -> name}
        end

        repeat 3 with $iteration
            Print "iteration {$iteration}"
        end
        """;

    public Result AcceptStatement(StatementContext context)
    {
        if (context is not IAcceptOptionalVariableDefinitionsContext receiver)
        {
            return $"{context} does not accept variable definitions.";
        }

        _receiver = new(receiver);
        return true;
    }

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        if (token is not VariableToken vToken)
        {
            return TryAddTokenRes.Error($"Value '{token.RawRep}' is not a variable.");
        }

        _variables.Add(vToken);
        return TryAddTokenRes.Continue();
    }

    public override Result VerifyCurrentState()
    {
        Result err = "The statement above does not accept provided variables.";
        if (_receiver.Value.SetOptionalVariables(_variables.ToArray()).HasErrored(out var error))
        {
            return err + error;
        }

        return Result.Assert(
            _variables.Count > 0,
            "No variables were provided."
        );
    }

    protected override void Execute()
    {
    }
}
