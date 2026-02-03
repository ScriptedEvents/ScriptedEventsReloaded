using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;

namespace SER.Code.MethodSystem.Methods.AdminToysMethods;

[UsedImplicitly]
// ReSharper disable InconsistentNaming
public class TPToyPlayerMethod : SynchronousMethod, IAdditionalDescription
{
    public override string Description => "Teleports an Admin Toy to a given player";

    public string AdditionalDescription => TPToyPosMethod.Singleton.AdditionalDescription;
    
    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<AdminToy>("toy reference"),
        new PlayerArgument("player to teleport toy to"),
        new BoolArgument("align toy rotation to player?"),
    ];

    public override void Execute()
    {
        var toy = Args.GetReference<AdminToy>("toy reference");
        var plr = Args.GetPlayer("player to teleport toy to");
        var alignRotation = Args.GetBool("align toy rotation to player?");
        
        TPToyPosMethod.TeleportOrSpawn(toy, plr.Position, alignRotation ? plr.Rotation : null);
    }
}