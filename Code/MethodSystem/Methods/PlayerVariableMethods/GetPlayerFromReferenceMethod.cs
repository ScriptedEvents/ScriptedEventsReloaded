using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.PlayerVariableMethods;

[UsedImplicitly]
public class GetPlayerFromReferenceMethod : ReturningMethod<PlayerValue>, IReferenceResolvingMethod
{
    public Type ReferenceType => typeof(Player);

    public override string Description => 
        "Returns a player variable with a single player from a reference.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<Player>("playerReference")
    ];

    public override void Execute()
    {
        ReturnValue = new PlayerValue(Args.GetReference<Player>("playerReference"));
    }
}