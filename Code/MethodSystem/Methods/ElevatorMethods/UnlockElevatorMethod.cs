using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.ElevatorMethods;

[UsedImplicitly]
public class UnlockElevatorMethod : SynchronousMethod
{
    public override string Description => "Unlocks elevators.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new ElevatorsArgument("elevators")    
    ];
    
    public override void Execute()
    {
        var elevators = Args.GetElevators("elevators");
        elevators.ForEach(el => el.UnlockAllDoors());
    }
}