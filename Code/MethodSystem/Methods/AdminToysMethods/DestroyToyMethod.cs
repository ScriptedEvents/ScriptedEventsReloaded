using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using Mirror;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;

namespace SER.Code.MethodSystem.Methods.AdminToysMethods;

[UsedImplicitly]
public class DestroyToyMethod : SynchronousMethod, ICanError
{
    public override string Description => "Destroys an Admin Toy.";

    public string[] ErrorReasons =>
    [
        "The Admin Toy hasn't been spawned yet or doesn't exist anymore."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<AdminToy>("toy reference")
    ];
    public override void Execute()
    {
        var toy = Args.GetReference<AdminToy>("toy reference");
        
        if (!NetworkServer.spawned.ContainsValue(toy.GameObject.NetworkIdentity))
            throw new ScriptRuntimeError(this, ErrorReasons[0]);
        
        toy.Destroy();
    }
}