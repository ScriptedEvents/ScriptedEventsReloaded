using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.MapMethods;

[UsedImplicitly]
public class CleanupPickupsMethod : SynchronousMethod
{
    public override string Description => "Cleans pickups (items) from the map.";

    public override Argument[] ExpectedArguments { get; } = [];
    
    public override void Execute()
    {
        Map.Pickups.ForEachItem(p => p.Destroy());
    }
}