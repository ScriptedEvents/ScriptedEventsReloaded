using Newtonsoft.Json.Linq;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;

namespace SER.Code.MethodSystem.Methods.HTTPMethods;

// ReSharper disable once InconsistentNaming
public class ParseJSONMethod : ReferenceReturningMethod<JObject>, ICanError
{
    public override string Description => "Parses a provided value into a JSON object.";

    public string[] ErrorReasons { get; } =
    [
        "Provided value is not parsable as a JSON object."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("string representation of JSON object")
    ];
    
    public override void Execute()
    {
        var valueToParse = Args.GetText("string representation of JSON object");
        
        try
        {
            ReturnValue = JObject.Parse(valueToParse);
        }
        catch
        {
            throw new ScriptRuntimeError(this, ErrorReasons[0]);
        }
    }
}