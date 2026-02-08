using JetBrains.Annotations;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.Documentation;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;

namespace SER.Code.ContextSystem.Contexts;

[UsedImplicitly]
public class WithContext : StandardContext, IKeywordContext, INotRunningContext, IRequireCurrentStatement
{
    private readonly List<VariableToken> _variables = [];
    private IAcceptOptionalVariableDefinitions _receiver = null!;
    
    public string KeywordName => "with";

    public string Description =>
        "This keyword is designed to provide a variable or a collection of variables to a statement. " +
        "Usually used for TEMPORARY variables in loops and functions, meaning they are removed after the statement ends.";

    public string[] Arguments => ["[variables...]"];

    public string ExampleUsage =>
        """
        # "with" keyword defines a temporary variable to count from 1 to 5
        repeat 5 
            with $index
            
            Reply $index
        end
        
        # some 
        """;

    public static DocLine GetDoc(params VariableToken[] variables)
    {
        return new DocLine([BaseToken.GetToken<KeywordToken>("with"), ..variables]);
    }

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

    protected override TryAddTokenRes OnAddingToken(BaseToken token)
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