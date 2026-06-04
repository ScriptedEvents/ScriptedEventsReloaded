using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.RoundMethods;

public class StartRoundMethod : SynchronousMethod, IEssential
{
    public override string Description => "Start a round.";

    public override Argument[] ExpectedArguments { get; } = [];
    
    public override void Execute()
    {
        Round.Start();
    }
}