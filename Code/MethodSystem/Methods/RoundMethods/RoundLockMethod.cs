using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.RoundMethods;

[UsedImplicitly]
public class RoundLockMethod : SynchronousMethod
{
    public override string Description => "Changes the round lock state.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new BoolArgument("new state")
    ];
    
    public override void Execute()
    {
        Round.IsLocked = Args.GetBool("new state");
    }
}