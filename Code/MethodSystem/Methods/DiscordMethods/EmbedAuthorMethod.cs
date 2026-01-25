using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.DiscordMethods;

[UsedImplicitly]
public class EmbedAuthorMethod : ReferenceReturningMethod<EmbedAuthorMethod.DEmbedAuthor>
{
    public class DEmbedAuthor : JObject;
    
    public override string Description => "Creates an author object that can be used in discord embeds.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("name")
        {
            Description = "Small text at the top of the embed."
        },
        new TextArgument("icon url")
        {
            DefaultValue = new(null, "none"), 
            Description = "Small round image next to the author name."
        },
        new TextArgument("clickable url")
        {
            DefaultValue = new(null, "none"),
            Description = "Link that turns the author name into a hyperlink."
        },
    ];
    
    public override void Execute()
    {
        var author = new DEmbedAuthor
        {
            ["name"] = Args.GetText("name")
        };
        
        if (Args.GetText("clickable url") is {} url) author["url"] = url;
        if (Args.GetText("icon url") is {} iconUrl) author["icon_url"] = iconUrl;
        
        ReturnValue = author;
    }
}