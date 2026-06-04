using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.MapMethods;

public class CleanupPickupsMethod : SynchronousMethod, IEssential
{
    public override string Description => "Cleans pickups (items) from the map.";

    public override Argument[] ExpectedArguments { get; } = [];
    
    public override void Execute()
    {
        Map.Pickups.ForEachItem(p => p.Destroy());
    }
}