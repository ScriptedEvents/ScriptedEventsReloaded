using Exiled.API.Enums;
using Exiled.API.Features;
using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class SetAmmoLimitMethod : SynchronousMethod, IExiledMethod
{
    public override string Description => "Sets the players' limit on a certain ammunition type";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players to set the limit to"),
        new EnumArgument<AmmoType>("ammo type"),
        new IntArgument("limit", 0, 65535)
        {
            Description = "The min and max are there so the whole thing doesn't crash from int -> ushort conversion :trollface:"
        }
    ];

    public override void Execute()
    {
        var ammoType = Args.GetEnum<AmmoType>("ammo type");
        var limit = (ushort) Args.GetInt("limit");
        var labApiPlayers = Args.GetPlayers("players to set the limit to");
        labApiPlayers.ForEach(plr => Player.Get(plr).SetAmmoLimit(ammoType, limit));
    }
}