using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.Arguments;

public class TextArgument(string name, bool needsQuotes = true, bool allowsSpaces = true) : Argument(name)
{
    public override string InputDescription => "Any text e.g. \"Hello, World!\""
        + (!needsQuotes ? " (it can be without the quotes)" : "")
        + (!allowsSpaces ? " but it CANNOT contain spaces!" : "");

    [UsedImplicitly]
    public DynamicTryGet<string> GetConvertSolution(BaseToken token)
    {
        if (token is TextToken textToken)
        {
            return new(() => textToken.GetDynamicResolver().Invoke().OnSuccess(SpaceCheck, null));
        }    
        
        if (token is not IValueToken valToken || !valToken.CapableOf<LiteralValue>(out var get))
        {
            if (!needsQuotes)
            {
                return SpaceCheck(token.GetBestTextRepresentation(null));
            }
            
            return DynamicTryGet.Error($"Value '{token.RawRep}' cannot represent text.");
        }

        if (valToken.IsConstant)
        {
            return SpaceCheck(get().OnSuccess(v => v.StringRep, null));
        }

        return new(() => get().OnSuccess(v => SpaceCheck(v.StringRep), null));
        
        TryGet<string> SpaceCheck(string value)
        {
            if (!allowsSpaces && value.Any(char.IsWhiteSpace))
            {
                return $"Value '{token.RawRep}' contains spaces, which are not allowed!".AsError();
            }
            
            return value.AsSuccess();
        }
    }
}