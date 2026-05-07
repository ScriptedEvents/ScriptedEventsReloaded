using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using UnityEngine;

namespace SER.Code.MethodSystem.Methods.AdminToysMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Toy_MoveMethod : SynchronousMethod, ICanError, IAdditionalDescription
{
    public override string Description => "Moves an Admin Toy relative to its rotation.";

    public string AdditionalDescription => $"E.g. \"{GetFriendlyName(typeof(Toy_MoveMethod))} *toy 0 0 1\" would " +
                                           $"move the referenced toy 1 unit (meter) forward.";
    public string[] ErrorReasons =>
    [
        "The Admin Toy hasn't been spawned yet or doesn't exist anymore."
    ];
    
    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<AdminToy>("toy reference"),
        
        new FloatArgument("x position"),
        new FloatArgument("y position"),
        new FloatArgument("z position"),
    ];
    
    public override void Execute()
    {
        var toy = Args.GetReference<AdminToy>("toy reference");
        var pos = new Vector3(
            Args.GetFloat("x position"),
            Args.GetFloat("y position"),
            Args.GetFloat("z position"));
        
        toy.Transform.Translate(pos);
    }

}