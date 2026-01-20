using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Contexts.VariableDefinition;
using SER.Code.ScriptSystem;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.TokenSystem.Tokens.VariableTokens;

public class CollectionVariableToken : VariableToken<CollectionVariable, CollectionValue>
{
    public static string Example => "&collection";

    public override Context GetContext(Script scr)
    {
        return new CollectionVariableDefinitionContext(this)
        {
            Script = scr,
            LineNum = LineNum,
        };
    }
}