using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.TokenSystem.Tokens;
using EventHandler = SER.Code.EventSystem.EventHandler;

namespace SER.Code.ArgumentSystem.Arguments;

public class EventArgument(string name) : Argument(name)
{
    public override string InputDescription =>
        "An in-game event e.g. RoundStarted (found using command 'serhelp events')";

    [UsedImplicitly]
    public OldDynamicTryGet<string> GetConvertSolution(BaseToken token)
    {
        OldResult rs = $"Value '{token.RawRep}' is not a valid event.";
        if (token.BestTextRepr().IsStatic(out var evName, out var get))
        {
            return Verify(evName);
        }

        return new(() => Verify(get()));
    }

    private static OldTryGet<string> Verify(string evName)
    {
        if (EventHandler.AvailableEvents.Any(info =>
                info.Name.Equals(evName, StringComparison.CurrentCultureIgnoreCase)))
        {
            return OldTryGet<string>.Success(evName);
        }

        return OldTryGet<string>.Error($"'{evName}' is not a valid event.");
    }
}