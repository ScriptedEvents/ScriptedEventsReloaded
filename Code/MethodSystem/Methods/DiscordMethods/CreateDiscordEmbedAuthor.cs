using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Methods.DiscordMethods.Structures;

namespace SER.Code.MethodSystem.Methods.DiscordMethods;

[UsedImplicitly]
public class CreateDiscordEmbedAuthor : ReferenceReturningMethod<DEmbedAuthor>
{
    public override string Description => "Creates an author object that can be used in discord embeds.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("name"),
        new TextArgument("url")
        {
            DefaultValue = new(null, "none")
        },
        new TextArgument("icon url")
        {
            DefaultValue = new(null, "none")
        },
        new TextArgument("proxy icon url")
        {
            DefaultValue = new(null, "none")
        }
    ];
    
    public override void Execute()
    {
        var author = new JObject
        {
            ["name"] = Args.GetText("name")
        };
        
        if (Args.GetText("url") is {} url) author["url"] = url;
        if (Args.GetText("icon url") is {} iconUrl) author["icon_url"] = iconUrl;
        if (Args.GetText("proxy icon url") is {} proxyIconUrl) author["proxy_icon_url"] = proxyIconUrl;
        
        ReturnValue = new(author);
    }
}