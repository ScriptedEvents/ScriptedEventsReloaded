using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.CommunicationInterfaces;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;

namespace SER.Code.ContextSystem.Contexts;

public class WithContext : StandardContext, IKeywordContext, INotRunningContext, IRequireCurrentStatement
{
    private readonly List<VariableToken> _variables = [];
    private IAcceptOptionalVariableDefinitions _receiver = null!;
    
    public string KeywordName => "with";

    public string Description =>
        "This keyword is designed to provide a variable or a collection of variables to a statement.";

    public string[] Arguments => ["[variables...]"];

    public Result AcceptStatement(StatementContext context)
    {
        if (context is not IAcceptOptionalVariableDefinitions receiver)
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