
using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using UnityEngine;

namespace SER.Code.MethodSystem.Methods.AdminToysMethods;

[UsedImplicitly]
public class SetToyRotationMethod : SynchronousMethod
{
    public override string Description => "Sets the rotation of a toy.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<AdminToy>("toy reference"),
        new OptionsArgument("rotation mode", "add", "set")
        {
            DefaultValue = new("add", null)
        },
        new FloatArgument("x rotation") { DefaultValue = new(0f, null) },
        new FloatArgument("y rotation") { DefaultValue = new(0f, null) },
        new FloatArgument("z rotation") { DefaultValue = new(0f, null) },
    ];
    
    public override void Execute()
    {
        var toy = Args.GetReference<AdminToy>("toy reference");
        var rotation = new Vector3(
            Args.GetFloat("x rotation"),
            Args.GetFloat("y rotation"),
            Args.GetFloat("z rotation")
        );
        
        toy.Rotation = Quaternion.Euler(
            Args.GetOption("rotation mode") == "add" 
                ? toy.Rotation.eulerAngles + rotation 
                : rotation
        );
    }
}