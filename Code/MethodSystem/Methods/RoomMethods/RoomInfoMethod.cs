using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using MapGeneration;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.ArgumentSystem.Structures;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.RoomMethods;

[UsedImplicitly]
public class RoomInfoMethod : LiteralValueReturningMethod, IReferenceResolvingMethod
{
    public Type ReferenceType => typeof(Room);
    
    public override TypeOfValue LiteralReturnTypes => new TypesOfValue([
        typeof(TextValue), 
        typeof(NumberValue)
    ]);

    public override string Description => IReferenceResolvingMethod.Desc.Get(this);

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<Room>("room"),
        new OptionsArgument("property",
            Option.Enum<RoomShape>("shape"),
            Option.Enum<RoomName>("name"), 
            Option.Enum<FacilityZone>("zone"),
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
            "shape" => new TextValue(room.Shape.ToString()),
            "name" => new TextValue(room.Name.ToString()),
            "zone" => new TextValue(room.Zone.ToString()),
            "xpos" => new NumberValue((decimal)room.Position.x),
            "ypos" => new NumberValue((decimal)room.Position.y),
            "zpos" => new NumberValue((decimal)room.Position.z),
            _ => throw new AndrzejFuckedUpException("room info property out of range")
        };
    }
}