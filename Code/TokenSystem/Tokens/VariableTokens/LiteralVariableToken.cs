using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Contexts.VariableDefinition;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.TokenSystem.Tokens.VariableTokens;

public class LiteralVariableToken : VariableToken<LiteralVariable, LiteralValue>
{
    public override Context? GetContext(Script? scr)
    {
        return new LiteralVariableDefinitionContext(this)
        {
            Script = scr,
            LineNum = LineNum,
        };
    }
    
    public TryGet<T> TryGetValue<T>() where T : Value
    {
        if (TryGetVariable().HasErrored(out var varError, out var variable))
        {
            return varError;
        }

        if (variable.TryGetValue<T>().HasErrored(out var valError, out var value))
        {
            return valError;
        }
                
        return value;
    }
}