using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;

namespace SER.Code.MethodSystem.Methods.CASSIEMethods;

[UsedImplicitly]
public class ClearCassieMethod : SynchronousMethod
{
    public override string Description => "Clears all CASSIE announcements, active or queued.";
    
    public override Argument[] ExpectedArguments { get; } = [];
    
    public override void Execute()
    {
        Announcer.Clear();
    }
}