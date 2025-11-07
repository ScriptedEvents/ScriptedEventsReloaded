using LabApi.Features.Wrappers;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;

namespace SER.MethodSystem.Methods.WarheadMethods;

public class DetonateWarheadMethod : SynchronousMethod
{
    public override string Description => "Detonates the alpha warhead.";
    
    public override Argument[] ExpectedArguments { get; } = [];
    
    public override void Execute()
    {
        Warhead.Detonate();
    }
}