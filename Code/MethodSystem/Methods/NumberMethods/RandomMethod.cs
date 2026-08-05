using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;
using SER.Code.ValueSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SER.Code.MethodSystem.Methods.NumberMethods;

[UsedImplicitly]
public class RandomMethod : ReturningMethod<NumberValue>, IAdditionalDescription, ICanError
{
    public override string Description =>
        "Returns a randomly generated number.";

    public string AdditionalDescription =>
        "'startingNum' argument MUST be smaller than 'endingNum' argument.";

    public string[] ErrorReasons =>
    [
        "The starting number must not be greater than the ending number.",
        "The requested integer range does not contain an integer."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new FloatArgument("startingNum"),
        new FloatArgument("endingNum"),
        new OptionsArgument(
            "numberType", 
            new("int", "Returns an integer number"), 
            new("real", "Returns a real number")
        )
        {
            DefaultValue = new("int", null)
        }
    ];

    public override void Execute()
    {
        Log.D("starting random num is running");
        var startingNum = Args.GetFloat("startingNum");
        var endingNum = Args.GetFloat("endingNum");
        var type = Args.GetOption("numberType");
        
        if (startingNum > endingNum)
            throw new SER.Code.Exceptions.ScriptRuntimeError(this, ErrorReasons[0]);

        if (type == "int")
        {
            var firstInteger = Mathf.CeilToInt(startingNum);
            var lastInteger = Mathf.FloorToInt(endingNum);
            if (firstInteger > lastInteger)
                throw new SER.Code.Exceptions.ScriptRuntimeError(this, ErrorReasons[1]);

            ReturnValue = Random.Range(firstInteger, lastInteger + 1);
            return;
        }

        var val = Random.Range(startingNum, endingNum);
        Log.D("random number returns " + val);
        ReturnValue = new NumberValue((decimal)val);
    }
}
