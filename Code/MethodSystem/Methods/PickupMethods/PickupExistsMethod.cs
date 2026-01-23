using LabApi.Features.Wrappers;
using Mirror;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.PickupMethods;

public class PickupExistsMethod : ReturningMethod<BoolValue>
{
    public override string Description => "Returns true if the provided pickup is still present on the server.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<Pickup>("pickup reference")
    ];

    public override void Execute()
    {
        var pickup = Args.GetReference<Pickup>("pickup reference");
        ReturnValue = NetworkServer.spawned.ContainsValue(pickup.NetworkIdentity);
    }
}