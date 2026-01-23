using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.HTTPMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class CreateJSONMethod : ReferenceReturningMethod<JObject>
{
    public override string Description => "Returns an empty JSON object.";

    public override Argument[] ExpectedArguments { get; } = [];
    
    public override void Execute()
    {
        ReturnValue = new JObject();
    }
}