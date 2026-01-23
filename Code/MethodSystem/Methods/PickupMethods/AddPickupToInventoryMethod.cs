using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;

namespace SER.Code.MethodSystem.Methods.PickupMethods;

[UsedImplicitly]
public class AddPickupToInventoryMethod : SynchronousMethod, IAdditionalDescription
{
    public override string Description => "Forces a pickup to be added to the player's inventory.";

    public string AdditionalDescription => "Pickup will not be added if the player's inventory is full.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayerArgument("player"),
        new ReferenceArgument<Pickup>("pickup")
    ];

    public override void Execute()
    {
        var player = Args.GetPlayer("player");
        var pickup = Args.GetReference<Pickup>("pickup");

        if (player.IsInventoryFull) return;
        
        player.AddItem(pickup);
    }
}