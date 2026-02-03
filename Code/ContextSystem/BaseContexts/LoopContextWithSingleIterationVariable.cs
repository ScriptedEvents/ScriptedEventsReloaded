using SER.Code.ContextSystem.CommunicationInterfaces;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.ContextSystem.BaseContexts;

public abstract class LoopContextWithSingleIterationVariable<TVal> : 
    LoopContext, 
    IAcceptOptionalVariableDefinitions 
    where TVal : Value
{
    private VariableToken? _iterationVariableToken;
    private Variable? _iterationVariable;
    
    public Result SetOptionalVariables(params VariableToken[] variableTokens)
    {
        if (variableTokens.FirstOrDefault() is not {} varToken) return true;

        if (!varToken.ValueType.IsAssignableFrom(typeof(TVal)))
        {
            return $"Provided variable '{varToken.RawRepr}' cannot be used for this loop, " +
                   $"as it cannot hold a {typeof(TVal).FriendlyTypeName()}";
        }
        
        _iterationVariableToken = varToken;
        return true;
    }
    
    protected void SetVariable(TVal value)
    {
        if (_iterationVariableToken is null) return;
        
        _iterationVariable = Variable.Create(_iterationVariableToken.Name, value);
        Script.AddVariable(_iterationVariable);
    }

    protected void RemoveVariable()
    {
        if (_iterationVariable is null) return;
        
        Script.RemoveVariable(_iterationVariable);
        _iterationVariable = null;
    }
}