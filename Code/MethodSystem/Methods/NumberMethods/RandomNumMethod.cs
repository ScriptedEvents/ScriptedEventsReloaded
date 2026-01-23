using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SER.Code.MethodSystem.Methods.NumberMethods;

[UsedImplicitly]
public class RandomNumMethod : ReturningMethod<NumberValue>, IAdditionalDescription
{
    public override string Description =>
        "Returns a randomly generated number.";

    public string AdditionalDescription =>
        "'startingNum' argument MUST be smaller than 'endingNum' argument.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new FloatArgument("startingNum"),
        new FloatArgument("endingNum"),
        new OptionsArgument(
            "numberType", 
            new("int", "Returns an integer number"), 
            new("real", "Returns a real number")
        )
    ];

    public override void Execute()
    {
        Log.D("starting random num is running");
        var startingNum = Args.GetFloat("startingNum");
        var endingNum = Args.GetFloat("endingNum");
        var type = Args.GetOption("numberType");
        
        var val = Random.Range(startingNum, endingNum);
        if (type == "int")
        {
            val = Mathf.RoundToInt(val);
        }
        
        Log.D("random number returns " + val);
        ReturnValue = new NumberValue((decimal)val);
    }
}