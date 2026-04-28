using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.VariableSystem;

namespace SER.Code.ContextSystem.Contexts;

[UsedImplicitly]
public class DeleteContext : StandardContext, IKeywordContext
{
    private VariableToken _variableToken = null!;
    public override string FriendlyName => "delete statement";
    
    public string KeywordName => "delete";
    public string Description => "Deletes a variable.";
    public string[] Arguments => ["variable to delete"];
    public string Example =>
        """
        func SomeFunction
            $temporaryVariable = 10
            # ... further implementation ...
            
            # delete variable that should not be accessed anymore
            delete $temporaryVariable
        end
        
        run SomeFunction
        """;
    
    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        if (token is not VariableToken varToken)
        {
            return TryAddTokenRes.Error($"Delete statement expects a variable to delete, not {token}");
        }
        
        _variableToken = varToken;
        return TryAddTokenRes.End();
    }

    public override Result VerifyCurrentState()
    {
        return Result.Assert(
            _variableToken is not null,
            "Variable to delete was not provided."
        );
    }

    protected override void Execute()
    {
        Script.RemoveLocalVariable(_variableToken);
        VariableIndex.RemoveGlobalVariable(_variableToken);
    }
}