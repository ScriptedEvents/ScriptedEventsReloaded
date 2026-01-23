using Interactables.Interobjects.DoorUtils;
using JetBrains.Annotations;
using LabApi.Features.Enums;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.ArgumentSystem.Structures;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.DoorMethods;

[UsedImplicitly]
public class DoorInfoMethod : LiteralValueReturningMethod, IReferenceResolvingMethod
{
    public Type ReferenceType => typeof(Door);
    
    public override TypeOfValue LiteralReturnTypes => new TypesOfValue([
        typeof(TextValue), 
        typeof(BoolValue), 
        typeof(NumberValue)
    ]);

    public override string Description => IReferenceResolvingMethod.Desc.Get(this);

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<Door>("door"),
        new OptionsArgument("info",
            "isOpen",
            "isClosed",
            "isLocked",
            "isUnlocked",
            Option.Enum<DoorName>("name"),
            "unityName",
            "remainingHealth",
            "maxHealth",
            Option.Enum<DoorPermissionFlags>("permissions")
        )
    ];

    public override void Execute()
    {
        var door = Args.GetReference<Door>("door");
        var info = Args.GetOption("info");
        
        ReturnValue = info switch
        {
            "name" => new TextValue(door.DoorName.ToString()),
            "unityname" => new TextValue(door.Base.name),
            "isopen" => new BoolValue(door.IsOpened),
            "isclosed" => new BoolValue(!door.IsOpened),
            "islocked" => new BoolValue(door.IsLocked),
            "isunlocked" => new BoolValue(!door.IsLocked),
            "remaininghealth" => new NumberValue(door is BreakableDoor bDoor ? (decimal)bDoor.Health : -1),
            "maxhealth" => new NumberValue(door is BreakableDoor bDoor ? (decimal)bDoor.MaxHealth : -1),
            "permissions" => new TextValue(door.Permissions.ToString()),
            _ => throw new AndrzejFuckedUpException()
        };
    }
}