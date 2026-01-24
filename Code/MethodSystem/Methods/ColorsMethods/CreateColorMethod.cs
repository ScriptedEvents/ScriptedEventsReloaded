using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using UnityEngine;

namespace SER.Code.MethodSystem.Methods.ColorsMethods;

[UsedImplicitly]
public class CreateColorMethod : ReferenceReturningMethod<Color>
{
    public override string Description => "Creates a color object.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new FloatArgument("red component", 0, 255),
        new FloatArgument("green component", 0, 255),
        new FloatArgument("blue component", 0, 255)
    ];
    
    public override void Execute()
    {
        var red = Args.GetFloat("red component");
        var green = Args.GetFloat("green component");
        var blue = Args.GetFloat("blue component");
        
        ReturnValue = new(red / 255f, green / 255f, blue / 255f);
    }
}