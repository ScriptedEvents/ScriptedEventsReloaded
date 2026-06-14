using LabApi.Features.Wrappers;
using NetworkManagerUtils.Dummies;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.DummyMethods;

[UsedImplicitly]
public class AddDummyMethod : ReturningMethod<PlayerValue>
{
    public override string Description => "Adds a dummy to the server";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("name")
    ];
    
    public override void Execute()
    { 
        ReturnValue = new(Player.Get(DummyUtils.SpawnDummy(Args.GetText("name"))));
    }
}