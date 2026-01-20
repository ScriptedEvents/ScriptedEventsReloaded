using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.MethodSystem.MethodDescriptors;

namespace SER.Code.MethodSystem.Methods.PickupMethods;

[UsedImplicitly]
public class SpawnPickupPlayerMethod : SynchronousMethod, ICanError
{
    public override string Description => "Spawns an item pickup / grenade on a player.";
    
    public string[] ErrorReasons => SpawnPickupPosMethod.Singleton.ErrorReasons;

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<Pickup>("pickup/projectile reference"),
        new PlayerArgument("player to spawn pickup on"),
    ];

    public override void Execute()
    {
        var obj = Args.GetReference<Pickup>("pickup/projectile reference");
        var plr = Args.GetPlayer("player to spawn pickup on");

        SpawnPickupPosMethod.SpawnPickup(obj, plr.Position, this);
    }
}