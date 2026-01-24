using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Methods.DiscordMethods.Structures;
using UnityEngine;

namespace SER.Code.MethodSystem.Methods.DiscordMethods;

[UsedImplicitly]
public class CreateDiscordEmbedMethod : ReferenceReturningMethod<DEmbed>
{
    public override string Description => "Creates an embed which can later be sent to discord via webhook.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("title")
        {
            DefaultValue = new(null, "none")
        },
        new TextArgument("description")
        {
            DefaultValue = new(null, "none")
        },
        new ColorArgument("color")
        {
            DefaultValue = new(new Color(0, 0, 0, 1), "default")
        },
        new ReferenceArgument<DEmbedFooter>("footer")
        {
            DefaultValue = new(null, "none")
        }
    ];
    
    public override void Execute()
    {
        var embed = new JObject();

        if (Args.GetText("title") is { } title) embed["title"] = title;
        if (Args.GetText("description") is { } description) embed["description"] = description;
        if (Args.GetColor("color") is { a: < 1 } color)
        {
            int r = Mathf.RoundToInt(color.r * 255);
            int g = Mathf.RoundToInt(color.g * 255);
            int b = Mathf.RoundToInt(color.b * 255);
            
            embed["color"] = (r << 16) | (g << 8) | b;
        }
        if (Args.GetReference<DEmbedFooter>("footer") is { } footer) embed["footer"] = footer.Footer;
        
        ReturnValue = new(embed);
    }
}