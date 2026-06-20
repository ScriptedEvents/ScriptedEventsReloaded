using SER.Code.ContextSystem.Interfaces;
using SER.Code.Extensions;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.ContextSystem.BaseContexts;

public abstract class LoopContextWithSingleIterationVariable<TVal> :
    LoopContext,
    IAcceptOptionalVariableDefinitionsContext
    where TVal : Value
{
    private readonly SingleTypeOfValue _valueType = typeof(TVal);
    private Variable? _iterationVariable;
    private VariableToken? _iterationVariableToken;

    public OldResult SetOptionalVariables(params VariableToken[] variableTokens)
    {
        if (variableTokens.FirstOrDefault() is not { } varToken) return true;

        if (!varToken.ValueType.CanHold(_valueType))
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
        Script.AddLocalVariable(_iterationVariable);
    }

    protected void RemoveVariable()
    {
        if (_iterationVariable is null) return;

        Script.RemoveLocalVariable(_iterationVariable);
        _iterationVariable = null;
    }
}