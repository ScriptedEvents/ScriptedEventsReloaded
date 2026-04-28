using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.PropertySystem;

namespace SER.Code.MethodSystem.Methods.PlayerVariableMethods;

[UsedImplicitly]
public class FilterMethod : ReturningMethod<PlayerValue>
{
    public override string Description => "Returns players which match the value for a given property.";

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
        var handler = ((IValueWithProperties.PropInfo<Player>)PlayerValue.PropertyInfoMap[playerProperty]).Func;

        ReturnValue = new(playersToFilter.Where(p => handler(p) == desiredValue));
    }
}