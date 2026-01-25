using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.DiscordMethods;

[UsedImplicitly]
public class EmbedFooterMethod : ReferenceReturningMethod<EmbedFooterMethod.DEmbedFooter>
{
    public class DEmbedFooter : JObject;
    
    public override string Description => "Creates a footer that can be used in discord embeds.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("content")
        {
            Description = "Small text at the bottom of the embed."
        },
        new TextArgument("icon url")
        {
            Description = "Small square image next to the footer text."
        }
    ];
    
    public override void Execute()
    {
        ReturnValue = new()
        {
            ["text"] = Args.GetText("content"),
            ["icon_url"] = Args.GetText("icon url")
        };
    }
}