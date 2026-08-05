using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.VariableSystem;

namespace SER.Code.ContextSystem.Contexts;

[UsedImplicitly]
public class DeleteKeyword : StandardContext, IKeywordContext
{
    private VariableToken _variableToken = null!;
    public override string FriendlyName => "delete statement";

    public string KeywordName => "delete";
    public string Description => "Deletes a variable.";
    public ContextArgument[] Arguments => [ContextArgument.Required(
        "$varToDelete", "Variable to remove from the local or global scope.",
        "An existing variable name such as $value or @players.")];
    public string Example =>
        """
        global $temporaryMessage = "delete me"
        delete $temporaryMessage

        # Local variables can be deleted the same way.
        $localValue = 42
        delete $localValue
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
