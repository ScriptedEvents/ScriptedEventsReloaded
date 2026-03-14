using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using MapGeneration.Distributors;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.DoorMethods;

[UsedImplicitly]
public class PryGateMethod : SynchronousMethod
{
    public override string Description => "Pries a gate.";

    public override Argument[] ExpectedArguments =>
    [
        new DoorArgument("gate"),
        new BoolArgument("should play effects")
        {
            DefaultValue = new(false, "does not play button effects"),
            Description = "Whether to play gate button effects when pried."
        }
    ];
    
    public override void Execute()
    {
        var door = Args.GetDoor("gate");
        var playEffects = Args.GetBool("should play effects");

        if (door is not Gate gate) return;

        if (gate.IsOpened || gate.ExactState != 0f || gate.Base.IsBeingPried) return;
        
        if (playEffects)
        {
            gate.PlayPermissionDeniedAnimation();
            gate.PlayLockBypassDeniedSound();
        }
        
        // Spawn pickups in case a player goes through the gate. This should not duplicate pickups if the gate gets opened properly later.
        SpawnablesDistributorBase.ServerSpawnForAllDoor(gate.Base);
        
        gate.Pry();
    }
}