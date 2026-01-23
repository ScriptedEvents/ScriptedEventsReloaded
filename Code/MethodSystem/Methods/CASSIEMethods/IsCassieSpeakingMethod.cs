using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.CASSIEMethods;

[UsedImplicitly]
public class IsCassieSpeakingMethod : ReturningMethod<BoolValue>
{
    public override string Description => "Returns boolean value indicating if CASSIE is speaking.";
    
    public override Argument[] ExpectedArguments { get; } = [];
    
    public override void Execute()
    {
        ReturnValue = Announcer.IsSpeaking;
    }
}