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
    public override string Description => 
        "Generates a random number; returns true if it is less than your specified percentage threshold.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new FloatArgument("chance", 0, 1)
    ];
    
    public override void Execute()
    {
        ReturnValue = Args.GetFloat("chance") < UnityEngine.Random.Range(0f, 1f);
    }

    public string AdditionalDescription =>
        "In simple terms, when you use {Chance 20%}, it will return true 20% of the time.";
}