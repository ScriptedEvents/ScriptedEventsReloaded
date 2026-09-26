using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.MapMethods;

[UsedImplicitly]
public class DestroyRagdollMethod : SynchronousMethod, ICanError
{
    public override string Description => "Destroys one ragdoll.";

    public string[] ErrorReasons => ["The ragdoll has already been destroyed."];

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<Ragdoll>("ragdoll")
    ];

    public override void Execute()
    {
        var ragdoll = Args.GetReference<Ragdoll>("ragdoll");
        if (ragdoll.IsDestroyed)
            throw new ScriptRuntimeError(this, ErrorReasons[0]);

        ragdoll.Destroy();
    }
}
