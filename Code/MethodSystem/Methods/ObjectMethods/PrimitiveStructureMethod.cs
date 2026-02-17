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

public class PrimitiveStructureMethod : ReferenceReturningMethod<GameObject>, ICanError
{
    public override string Description { get; } = "Changes the position of primitive.";
    public override Argument[] ExpectedArguments { get; } = 
    [
        new ReferenceArgument<GameObject>("object")
        {
            Description = "The GameObject that you want to change Position/Rotation/Scale of",
        },
        new OptionsArgument("option", "Position, Rotation, Scale"),
        new FloatArgument("x"),
        new FloatArgument("y"),
        new FloatArgument("z"),
    ];
    public override void Execute()
    {
        var obj = Args.GetReference<GameObject>("object");
        ReturnValue = obj;
        var option = Args.GetOption("option");
        var x =  Args.GetFloat("x");
        var y =  Args.GetFloat("y");
        var z =  Args.GetFloat("z");
        var vector = new Vector3(x, y, z);
        
        var adminToy = AdminToy.Get(obj.GetComponent<AdminToyBase>());
        if (adminToy is not PrimitiveObjectToy primitiveObjectToy)
        {
            Script.Executor.Error(ErrorReasons[0], Script);
            return;
        }

        switch (option)
        {
            case "position":
                primitiveObjectToy.Position = vector;
                break;
            case "rotation":
                primitiveObjectToy.Rotation = Quaternion.Euler(vector);
                break;
            case "scale":
                primitiveObjectToy.Scale = vector;
                break;
        }
    }

    public string[] ErrorReasons { get; } = 
    [
        "Not all gameObjects can change colour. This one is not Primitive (cube, sphere, circle, cylinder, etc.)"
    ];
}