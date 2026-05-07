using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;

namespace SER.Code.MethodSystem.Methods.AdminToysMethods;

[UsedImplicitly]
// ReSharper disable InconsistentNaming
public class Toy_TPRoomMethod : SynchronousMethod, IAdditionalDescription
{
    public override string Description => "Teleports an Admin Toy to the given room";

    public string AdditionalDescription => Toy_TPPositionMethod.Singleton.AdditionalDescription;
    
    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<AdminToy>("toy reference"),
        new RoomArgument("room to teleport toy to"),
        
        new FloatArgument("relative x") { DefaultValue = new(0, null) },
        new FloatArgument("relative y") { DefaultValue = new(0, null) },
        new FloatArgument("relative z") { DefaultValue = new(0, null) },
    ];

    public override void Execute()
    {
        var toy = Args.GetReference<AdminToy>("toy reference");
        var room = Args.GetRoom("room to teleport toy to");
        var pos = room.Transform.TransformPoint(
            Args.GetFloat("x position"),
            Args.GetFloat("y position"),
            Args.GetFloat("z position"));
        
        Toy_TPPositionMethod.TeleportOrSpawn(toy, pos);
    }
}