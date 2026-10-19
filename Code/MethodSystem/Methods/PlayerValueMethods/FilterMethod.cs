using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.PropertySystem;

namespace SER.Code.MethodSystem.Methods.PlayerValueMethods;

[UsedImplicitly]
public class FilterMethod : ReturningMethod<PlayerValue>, ICanError
{
    public override string Description => "Returns players which match the value for a given property.";

    public string[] ErrorReasons =>
    [
        "The chosen property cannot be used to filter players. Pick a real player property, not 'none'."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players to filter"),
        new EnumArgument<PlayerValue.PlayerProperty>("player property to filter by"),
        new AnyValueArgument("desired value of property")
    ];
    
    public override void Execute()
    {
        var playersToFilter = Args.GetPlayers("players to filter");
        var playerProperty = Args.GetEnum<PlayerValue.PlayerProperty>("player property to filter by");
        var desiredValue = Args.GetAnyValue("desired value of property");
        if (!PlayerValue.PropertyInfoMap.TryGetValue(playerProperty, out var propInfo) ||
            propInfo is not IValueWithProperties.PropInfo<Player> { Func: var handler })
        {
            throw new ScriptRuntimeError(this, ErrorReasons[0]);
        }

        ReturnValue = new(playersToFilter.Where(p => handler(p) == desiredValue));
    }
}