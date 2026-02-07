using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Contexts.VariableDefinition;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.TokenSystem.Tokens.VariableTokens;

public class CollectionVariableToken : VariableToken<CollectionVariable, CollectionValue>, ITraversableValueToken
{
    public static string Example => "&myCollectionVariable";

    public override Context? GetContext(Script? scr)
    {
        return new CollectionVariableDefinitionContext(this)
        {
            Script = scr,
            LineNum = LineNum,
        };
    }

    public TryGet<Value[]> GetTraversableValues()
    {
        return ExactValue.OnSuccess(val => val.CastedValues, null);
    }
}