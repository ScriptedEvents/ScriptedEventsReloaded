
using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using UnityEngine;

namespace SER.Code.MethodSystem.Methods.AdminToysMethods;

[UsedImplicitly]
public class SetToyScaleMethod : SynchronousMethod
{
    public override string Description => "Sets the scale of a toy.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<AdminToy>("toy reference"),
        new OptionsArgument("scale mode", "add", "set")
        {
            DefaultValue = new("add", null)
        },
        new FloatArgument("x scale") { DefaultValue = new(0f, null) },
        new FloatArgument("y scale") { DefaultValue = new(0f, null) },
        new FloatArgument("z scale") { DefaultValue = new(0f, null) },
    ];
    
    public override void Execute()
    {
        var toy = Args.GetReference<AdminToy>("toy reference");
        var scale = new Vector3
        {
            x = Args.GetFloat("x scale"),
            y = Args.GetFloat("y scale"),
            z = Args.GetFloat("z scale")
        };
        
        toy.Scale = Args.GetOption("scale mode") == "add" 
            ? toy.Scale + scale 
            : scale;
    }
}