using SER.Code.ContextSystem.Interfaces;
using SER.Code.Exceptions;
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
            return $"Provided variable '{varToken.RawRep}' cannot be used for this loop, " +
                   $"as it cannot hold a {typeof(TVal).FriendlyTypeName()}";
        }
        
        _iterationVariableToken = varToken;
        return true;
    }

    public TypeOfValue[] OptionalVariableTypes => [new TypeOfValue<TVal>()];

    protected void SetVariable(TVal value)
    {
        if (Script is null) throw new AnonymousUseException("LoopContextWithSingleIterationVariable.cs");
        if (_iterationVariableToken is null) return;
        
        _iterationVariable = Variable.Create(_iterationVariableToken.Name, value);
        Script.AddVariable(_iterationVariable);
    }

    protected void RemoveVariable()
    {
        if (Script is null) throw new AnonymousUseException("LoopContextWithSingleIterationVariable.cs");
        if (_iterationVariable is null) return;
        
        Script.RemoveVariable(_iterationVariable);
        _iterationVariable = null;
    }
}