using AdminToys;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.ArgumentSystem.Structures;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using UnityEngine;
using PrimitiveObjectToy = LabApi.Features.Wrappers.PrimitiveObjectToy;

namespace SER.Code.MethodSystem.Methods.ObjectMethods;

public class GetPrimitiveStructureMethod : ReferenceReturningMethod<Vector3>, ICanError
{
    public override string Description { get; } = "Changes the position of primitive.";
    public override Argument[] ExpectedArguments { get; } = 
    [
        new ReferenceArgument<GameObject>("object")
        {
            Description = "The GameObject that you want to change Position/Rotation/Scale of",
        },
        new OptionsArgument("option", "Position, Rotation, Scale"),
    ];
    public override void Execute()
    {
        var obj = Args.GetReference<GameObject>("object");
        var option = Args.GetOption("option");
        
        var adminToy = AdminToy.Get(obj.GetComponent<AdminToyBase>());
        if (adminToy is not PrimitiveObjectToy primitiveObjectToy)
        {
            Script.Executor.Error(ErrorReasons[0], Script);
            return;
        }

        switch (option)
        {
            case "position":
                ReturnValue = primitiveObjectToy.Position;
                break;
            case "rotation":
                ReturnValue = primitiveObjectToy.Rotation.eulerAngles;
                break;
            case "scale":
                ReturnValue = primitiveObjectToy.Scale;
                break;
        }
    }

    public string[] ErrorReasons { get; } = 
    [
        "Not all gameObjects can change colour. This one is not Primitive (cube, sphere, circle, cylinder, etc.)"
    ];
}