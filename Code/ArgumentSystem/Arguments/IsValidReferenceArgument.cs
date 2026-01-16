using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.Arguments;

public class IsValidReferenceArgument(string name) : Argument(name)
{
    public override string InputDescription => "a reference we want to check is valid e.g. @room";
    
    [UsedImplicitly]
    public DynamicTryGet<bool> GetConvertSolution(BaseToken token)
    {
        if (!token.CanReturn<ReferenceValue>(out var func))
        {
            return $"Value '{token.RawRep}' is not a reference.";
        }
        
        return new(() => func().OnSuccess(v => v.IsValid, null));
    }
}