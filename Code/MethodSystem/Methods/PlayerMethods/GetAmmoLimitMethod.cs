using Exiled.API.Enums;
using Exiled.API.Features;
using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.MethodSystem.Structures;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class GetAmmoLimitMethod : ReturningMethod<NumberValue>, IExiledMethod
{
    public override string Description => "Gets the player's limit on a certain ammunition type";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayerArgument("player to get the limit from"),
        new EnumArgument<AmmoType>("ammo type")
    ];

    public override void Execute()
    {
        var ammoType = Args.GetEnum<AmmoType>("ammo type");
        var player = Args.GetPlayer("player to get the limit from");
        
        ReturnValue = Player.Get(player).GetAmmoLimit(ammoType);
    }
}