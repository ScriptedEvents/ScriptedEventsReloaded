using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.Arguments;

public class AnyValueArgument(string name) : Argument(name)
{
    public override string InputDescription => "Any value";
    
    [UsedImplicitly]
    public DynamicTryGet<Value> GetConvertSolution(BaseToken token)
    {
        if (token is IValueToken valToken)
        {
            return new(valToken.Value);
        }
        
        return $"Value '{token.RawRep}' does not represent any kind of value";
    }
}