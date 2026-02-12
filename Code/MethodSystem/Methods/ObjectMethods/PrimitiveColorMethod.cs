using AdminToys;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using UnityEngine;
using PrimitiveObjectToy = LabApi.Features.Wrappers.PrimitiveObjectToy;

namespace SER.Code.MethodSystem.Methods.ObjectMethods;

public class PrimitiveColorMethod : ReferenceReturningMethod<GameObject>, ICanError
{
    public override string Description { get; } = "Changes the color of primitive.";
    public override Argument[] ExpectedArguments { get; } = 
        [
            new ReferenceArgument<GameObject>("object"),
            new ColorArgument("color")
        ];
    public override void Execute()
    {
        var obj = Args.GetReference<GameObject>("object");
        ReturnValue = obj;
        var color = Args.GetColor("color");

        var adminToy = AdminToy.Get(obj.GetComponent<AdminToyBase>());
        if (adminToy is not PrimitiveObjectToy primitiveObjectToy)
        {
            Script.Executor.Error(ErrorReasons[0], Script);
            return;
        }
        
        primitiveObjectToy.Color = color;
    }

    public string[] ErrorReasons { get; } = 
        [
            "Not all gameObjects can change colour. This one is not Primitive (cube, sphere, circle, cylinder, etc.)"
        ];
}