using JetBrains.Annotations;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Contexts.VariableDefinition;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.VariableSystem;

namespace SER.Code.ContextSystem.Contexts;

[UsedImplicitly]
public class GlobalVariableContext : YieldingContext, IKeywordContext
{
    protected override string FriendlyName =>
        $"global{(_variableToken is null ? "" : $" '{_variableToken.RawRep}'")} variable definition";

    public string KeywordName => "global";
    public string Description => "Creates/modifies a global variable.";
    public string[] Arguments => ["[variable prefix and name]", "=", "[value]"];
    public string? Example => null;

    private VariableDefinitionContext _variableContext = null!;
    private VariableToken? _variableToken;
    
    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        if (_variableToken is not null) return _variableContext.TryAddToken(token);
        
        if (token is not VariableToken variableToken)
            return TryAddTokenRes.Error($"{KeywordName} expects a variable definition afterwards.");

        _variableToken = variableToken;
        _variableContext = variableToken.GetContext(Script) as VariableDefinitionContext ?? throw new TosoksFuckedUpException();
        return TryAddTokenRes.Continue();
    }

    public override Result VerifyCurrentState()
    {
        if (_variableToken is null)
            return "Variable name and value haven't been provided.";
            
        return _variableContext.VerifyCurrentState();
    }

    protected override IEnumerator<float> Execute()
    {
        using var definitionEnumerator = _variableContext.Run();
        while (definitionEnumerator.MoveNext()) yield return definitionEnumerator.Current;
        VariableIndex.AddGlobalVariable(_variableContext.DefinedVariable ?? throw new TosoksFuckedUpException());
    }
}