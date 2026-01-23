using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.ReferenceVariableMethods;

[UsedImplicitly]
public class ValidRefMethod : ReturningMethod<BoolValue>
{
    public override string Description => "Verifies if the reference is valid, by checking if the object exists.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new IsValidReferenceArgument("reference")
    ];
    
    public override void Execute()
    {
        ReturnValue = Args.GetIsValidReference("reference");
    }
}