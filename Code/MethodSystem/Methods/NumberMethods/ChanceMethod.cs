using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.NumberMethods;

[UsedImplicitly]
public class ChanceMethod : ReturningMethod<BoolValue>, IAdditionalDescription
{
    public override string Description => "Returns true with the specified chance.";

    public string AdditionalDescription => "e.g. if set to 20%, true will be returned 20% of the time.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new FloatArgument("chance", 0, 1)
    ];

    public override void Execute()
    {
        ReturnValue = Args.GetFloat("chance") < UnityEngine.Random.Range(0f, 1f);
    }
}