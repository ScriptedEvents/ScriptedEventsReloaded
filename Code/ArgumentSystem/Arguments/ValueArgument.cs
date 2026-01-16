using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.Arguments;

public class ValueArgument<T>(string name) : Argument(name) where T : Value
{
    public override string InputDescription => $"a value of type {typeof(T).GetAccurateName()}";
    
    [UsedImplicitly]
    public DynamicTryGet<T> GetConvertSolution(BaseToken token)
    {
        if (!token.CanReturn<T>(out var get))
        {
            return $"Value '{token.RawRep}' cannot represent {InputDescription}";
        }
        
        return new(() => get());
    }
}