using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;

namespace SER.Code.MethodSystem.Methods.ElevatorMethods;

[UsedImplicitly]
public class SendElevatorMethod : SynchronousMethod
{
    public override string Description => "Sends elevators to the next floor.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new ElevatorsArgument("elevators")
    ];
    
    public override void Execute()
    {
        var elevators = Args.GetElevators("elevators");
        elevators.ForEach(el => el.SendToNextFloor());
    }
}