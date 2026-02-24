using JetBrains.Annotations;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;

namespace SER.Code.ContextSystem.Contexts;

[UsedImplicitly]
public class WithKeyword : StandardContext, IKeywordContext, INotRunningContext, IRequirePreviousStatementContext
{
    private readonly List<VariableToken> _variables = [];
    private IAcceptOptionalVariableDefinitionsContext _receiver = null!;
    
    public string KeywordName => "with";

    public string Description =>
        "This keyword is designed to provide a variable or a collection of variables to a statement.";

    public string[] Arguments => ["[variables...]"];

    public string Example =>
        """
        # CORRECT
        over @all
            with @plr

            Print {@plr name}
        end

        # WRONG - "with" keyword does not add indentation
        over @all
            with @plr
                Print {@plr name}
        end

        # WRONG - with keyword is not a statement that can be closed
        # this causes an error
        # over @all
        #     with @plr
        #         Print {@plr name}
        #     end
        # end
        """;

    public Result AcceptStatement(StatementContext context)
    {
        if (context is not IAcceptOptionalVariableDefinitionsContext receiver)
        {
            return $"{context} does not accept variable definitions.";
        }

        _receiver = receiver;
        return true;
    }

    protected override string FriendlyName => "'with' keyword";

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
        if (_receiver.SetOptionalVariables(_variables.ToArray()).HasErrored(out var error))
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