using LabApi.Features.Wrappers;
using MapGeneration;
using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.ArgumentSystem.Structures;
using SER.Helpers.Exceptions;
using SER.MethodSystem.BaseMethods;
using SER.MethodSystem.MethodDescriptors;
using SER.ValueSystem;

namespace SER.MethodSystem.Methods.RoomMethods;

public class RoomInfoMethod : LiteralValueReturningMethod, IReferenceResolvingMethod
{
    public Type ReferenceType => typeof(Room);
    public override Type[] LiteralReturnTypes => [typeof(TextValue), typeof(NumberValue)];

    public override string Description => null!;

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<Room>("room"),
        new OptionsArgument("property",
            Option.Enum<RoomShape>(),
            Option.Enum<RoomName>(), 
            Option.Enum<FacilityZone>(),
            "xPos",
            "yPos",
            "zPos"
        )
    ];
    
    public override void Execute()
    {
        var room = Args.GetReference<Room>("room");
        ReturnValue = Args.GetOption("property") switch
        {
            "roomshape" => new TextValue(room.Shape.ToString()),
            "roomname" => new TextValue(room.Name.ToString()),
            "facilityzone" => new TextValue(room.Zone.ToString()),
            "xpos" => new NumberValue((decimal)room.Position.x),
            "ypos" => new NumberValue((decimal)room.Position.y),
            "zpos" => new NumberValue((decimal)room.Position.z),
            _ => throw new AndrzejFuckedUpException("room info property out of range")
        };
    }
}