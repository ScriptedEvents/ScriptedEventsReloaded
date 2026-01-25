using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.DiscordMethods;

[UsedImplicitly]
public class EmbedFieldMethod : ReferenceReturningMethod<EmbedFieldMethod.DEmbedField>
{
    public class DEmbedField : JObject;
    
    public override string Description => "Creates a field object that can be used in discord embeds.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("name")
        {
            Description = "The bold \"header\" of a specific field."
        },
        new TextArgument("value")
        {
            Description = "The main text content for that specific field.",
        },
        new BoolArgument("inline")
        {
            Description = "Determines if fields sit side-by-side."
        }
    ];
    
    public override void Execute()
    {
        ReturnValue = new DEmbedField
        {
            ["name"] = Args.GetText("name"),
            ["value"] = Args.GetText("value"),
            ["inline"] = Args.GetBool("inline")
        };
    }
}