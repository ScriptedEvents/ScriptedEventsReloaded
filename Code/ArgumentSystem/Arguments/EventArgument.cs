using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using EventHandler = SER.Code.EventSystem.EventHandler;

namespace SER.Code.ArgumentSystem.Arguments;

public class EventArgument(string name) : Argument(name)
{
    public override string InputDescription => 
        "An in-game event e.g. RoundStarted (found using command 'serhelp events')";
    
    [UsedImplicitly]
    public DynamicTryGet<string> GetConvertSolution(BaseToken token)
    {
        Result rs = $"Value '{token.RawRep}' is not a valid event.";
        if (token.BestTextRepr().IsStatic(out var evName, out var get))
        {
            return Verify(evName);
        }
        
        return new(() => Verify(get()));
    }

    private static TryGet<string> Verify(string evName)
    {
        if (EventHandler.AvailableEvents.Any(info =>
                info.Name.Equals(evName, StringComparison.CurrentCultureIgnoreCase)))
        {
            return TryGet<string>.Success(evName);
        }

        return TryGet<string>.Error($"'{evName}' is not a valid event.");
    }
}