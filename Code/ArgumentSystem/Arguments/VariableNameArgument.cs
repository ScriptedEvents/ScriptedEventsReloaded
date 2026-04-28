using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;

namespace SER.Code.ArgumentSystem.Arguments;

public class VariableNameArgument(string name) : Argument(name)
{
    public override string InputDescription => "A variable name (doesn't have to be real)";

    [UsedImplicitly]
    public DynamicTryGet<VariableToken> GetConvertSolution(BaseToken token)
    {
        if (token is not VariableToken variableToken)
        {
            return $"Value '{token.RawRep}' is not a syntactically valid variable name.";
        }

        return variableToken;
    }
}

public class VariableNameArgument<T>(string name) : Argument(name)
    where T : VariableToken
{
    public string TypeName => typeof(T).AccurateName[..^"VariableToken".Length].LowerFirst();
    
    public override string InputDescription => $"A {TypeName} variable name (doesn't have to be real)";
    
    [UsedImplicitly]
    public DynamicTryGet<T> GetConvertSolution(BaseToken token)
    {
        if (token is not T variableToken)
        {
            return $"Value '{token.RawRep}' is not a syntactically valid {TypeName} variable name.";
        }
        
        return variableToken;
    }
}