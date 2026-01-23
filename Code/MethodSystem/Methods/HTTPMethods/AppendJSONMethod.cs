using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.HTTPMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class AppendJSONMethod : ReferenceReturningMethod<JObject>, ICanError
{
    public override string Description => "Parses a provided value into a JSON object.";

    public string[] ErrorReasons { get; } =
    [
        "Provided value was not able to be added to the JSON object."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<JObject>("JSON to add value to"),
        new TextArgument("key")
        {
            Description = "You will be using this key to refer to this value, like { \"key\": value }"
        },
        new ValueArgument<LiteralValue>("value")
    ];
    
    public override void Execute()
    {
        var jsonToAddValueTo = Args.GetReference<JObject>("JSON to add value to");
        var key = Args.GetText("key");
        var value = Args.GetValue<LiteralValue>("value");
        
        if (value is TextValue textValue)
        {
            jsonToAddValueTo[key] = textValue.ParsedValue(Script);
            return;
        }

        try
        {
            jsonToAddValueTo[key] = JToken.FromObject(value.Value);
        }
        catch
        {
            throw new ScriptRuntimeError(this, ErrorReasons[0]);
        }
    }
}