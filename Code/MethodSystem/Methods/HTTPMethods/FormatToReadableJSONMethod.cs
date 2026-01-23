using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.HTTPMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class FormatToReadableJSONMethod : ReturningMethod<TextValue>
{
    public override string Description => "Returns the JSON object in a readable format.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<JObject>("JSON object")
    ];
    
    public override void Execute()
    {
        ReturnValue = Args.GetReference<JObject>("JSON object").ToString(Newtonsoft.Json.Formatting.Indented);
    }
}