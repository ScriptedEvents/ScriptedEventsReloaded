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
public class JSONInfoMethod : ReturningMethod, IReferenceResolvingMethod, ICanError
{
    public override string Description => IReferenceResolvingMethod.Desc.Get(this);
    
    public override TypeOfValue Returns { get; } = new UnknownTypeOfValue();
    
    public Type ReferenceType { get; } = typeof(JObject);

    public string[] ErrorReasons { get; } =
    [
        "Provided JSON object does not contain the provided key.",
        "Provided value is not a JSON object, cannot get value from it."
    ];
    
    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<JObject>("JSON object"),
        new TextArgument("key")
        {
            ConsumesRemainingValues = true, 
            Description = "The key to get the value from. You can use multiple keys to get value from nested objects."
        }
    ];
    
    public override void Execute()
    {
        var obj = Args.GetReference<JObject>("JSON object");
        var keys = Args.GetRemainingArguments<string, TextArgument>("key");
        
        JToken currentToken = obj;
        foreach (string key in keys)
        {
            if (currentToken is not JObject nestedObj)
            {
                throw new ScriptRuntimeError(this, ErrorReasons[1]);
            }

            if (!nestedObj.TryGetValue(key, out JToken? nextToken))
            {
                throw new ScriptRuntimeError(this, ErrorReasons[0]);
            }

            currentToken = nextToken;
        }

        if (currentToken is JValue { Value: {} val })
        {
            ReturnValue = Value.Parse(val);
            return;
        }
        
        ReturnValue = Value.Parse(currentToken);
    }
}