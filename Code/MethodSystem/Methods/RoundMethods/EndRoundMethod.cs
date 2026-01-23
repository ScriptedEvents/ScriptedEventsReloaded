using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.RoundMethods;

[UsedImplicitly]
public class EndRoundMethod : SynchronousMethod
{
    public override string Description => "Ends a round.";

    public override Argument[] ExpectedArguments { get; } = [];
    
    public override void Execute()
    {
        Round.End(true);
    }
}