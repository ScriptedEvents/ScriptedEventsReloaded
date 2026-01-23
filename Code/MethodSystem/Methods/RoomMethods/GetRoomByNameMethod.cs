using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using MapGeneration;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.RoomMethods;

[UsedImplicitly]
public class GetRoomByNameMethod : ReferenceReturningMethod, IAdditionalDescription
{
    public override Type ReturnType => typeof(Room);
    
    public override string Description => "Returns a reference to a room which has the provided name.";

    public string AdditionalDescription =>
        "If more than one room matches the provided name, a random room will be returned.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new EnumArgument<RoomName>("room name")
    ];

    public override void Execute()
    {
        var roomName = Args.GetEnum<RoomName>("room name");
        var room = Room.List.Where(r => r.Name == roomName).GetRandomValue();
        if (room is null)
        {
            throw new ScriptRuntimeError(this, $"No room found with the provided name '{roomName}'.");
        }
        
        ReturnValue = new ReferenceValue(room);
    }
}