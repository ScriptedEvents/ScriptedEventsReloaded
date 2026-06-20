using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Contexts.VariableDefinition;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.VariableSystem;

namespace SER.Code.ContextSystem.Contexts;

[UsedImplicitly]
public class GlobalKeyword : YieldingContext, IKeywordContext
{
    private VariableDefinitionContext _variableContext = null!;
    private VariableToken? _variableToken;
    public override string FriendlyName =>
        $"global{(_variableToken is null ? "" : $" '{_variableToken.RawRep}'")} variable definition";

    public string KeywordName => "global";
    public string Description => "Creates/modifies a global variable.";
    public string[] Arguments => ["[variable prefix and name]", "=", "[value]"];
    public string? Example => null;

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        if (_variableToken is not null) return _variableContext.TryAddToken(token);

        if (token is not VariableToken variableToken)
            return TryAddTokenRes.Error($"{KeywordName} expects a variable definition afterwards.");

        _variableToken = variableToken;
        _variableContext = variableToken.GetContext(Script) as VariableDefinitionContext ?? throw new TosoksFuckedUpException();
        return TryAddTokenRes.Continue();
    }

    public override OldResult VerifyCurrentState()
    {
        if (_variableToken is null)
            return "Variable name and value haven't been provided.";

        return _variableContext.VerifyCurrentState();
    }

    protected override IEnumerator<float> Execute()
    {
        _variableContext.CreateLocalVariable = false;

        using var definitionEnumerator = _variableContext.Run();
        while (definitionEnumerator.MoveNext()) yield return definitionEnumerator.Current;

        if (_variableContext.DefinedVariable is null)
        {
            throw new TosoksFuckedUpException();
        }

        GlobalVariables.AddGlobalVariable(_variableContext.DefinedVariable);
    }
}