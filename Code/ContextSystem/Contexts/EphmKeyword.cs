using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Contexts.VariableDefinition;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;

namespace SER.Code.ContextSystem.Contexts;

[UsedImplicitly]
public class EphmKeyword : YieldingContext, IKeywordContext
{
    private VariableDefinitionContext _variableContext = null!;
    private VariableToken? _variableToken;
    public override string FriendlyName =>
        $"ephemeral{(_variableToken is null ? "" : $" '{_variableToken.RawRep}'")} variable definition";

    public string KeywordName => "ephm";
    public string Description => "Creates/modifies a ephemeral variable.";
    public ContextArgument[] Arguments => ["[variable prefix and name]", "=", "[value]"];
    public string Example =>
        """
        if {@sender -> isAlive}
            ephm $message = "Only needed during this statement"
            Print $message
        end
        
        # accessing $message outside that if statement will not work!
        # Print $message <-- will error
        """;

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        if (_variableToken is not null) return _variableContext.TryAddToken(token);

        if (token is not VariableToken variableToken)
            return TryAddTokenRes.Error($"{KeywordName} expects a variable definition afterwards.");

        _variableToken = variableToken;
        _variableContext = variableToken.GetContext(Script) as VariableDefinitionContext ?? throw new ExecutionInvariantException();
        return TryAddTokenRes.Continue();
    }

    public override Result VerifyCurrentState()
    {
        if (ParentContext is null) return "To define an ephemeral variable, it must be inside a statement.";

        if (_variableToken is null)
            return "Variable name and value haven't been provided.";

        return _variableContext.VerifyCurrentState();
    }

    protected override IEnumerator<float> Execute()
    {
        using var definitionEnumerator = _variableContext.Run();
        while (definitionEnumerator.MoveNext()) yield return definitionEnumerator.Current;

        if (_variableContext.DefinedVariable is null)
        {
            throw new ExecutionInvariantException();
        }

        ParentContext?.MarkVariableAsEphemeral(_variableContext.DefinedVariable);
    }
}
