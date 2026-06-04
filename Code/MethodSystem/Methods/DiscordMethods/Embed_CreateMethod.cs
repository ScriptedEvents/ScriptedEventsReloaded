using Newtonsoft.Json.Linq;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;
using UnityEngine;

namespace SER.Code.MethodSystem.Methods.DiscordMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Embed_CreateMethod : ReferenceReturningMethod<Embed_CreateMethod.DEmbed>, IAdditionalDescription
{
    public class DEmbed : JObject;
    
    public override string Description => "Creates an embed which can later be sent to discord via webhook.";

    public string AdditionalDescription => 
        $"This method does NOT send the embed. Use {NameOfMethod(typeof(Discord_SendMessageMethod))} for that.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("title")
        {
            Description = "The bold header text at the top.",
            DefaultValue = new(null, "none")
        },
        new TextArgument("description")
        {
            DefaultValue = new(null, "none"),
            Description = "The main body text of the embed."
        },
        new ColorArgument("color")
        {
            DefaultValue = new(new Color(0, 0, 0, 0), "none"),
            Description = "The vertical sidebar color."
        },
        new ReferenceArgument<Embed_CreateAuthorMethod.DEmbedAuthor>("author")
        {
            DefaultValue = new(null, "none"),
            Description = $"Created using {NameOfMethod(typeof(Embed_CreateAuthorMethod))}"
        },
        new ReferenceArgument<Embed_CreateFooterMethod.DEmbedFooter>("footer")
        {
            DefaultValue = new(null, "none"),
            Description = $"Created using {NameOfMethod(typeof(Embed_CreateFooterMethod))}"
        },
        new TextArgument("thumbnail url")
        {
            DefaultValue = new(null, "none"),
            Description = "The source link for the thumbnail."
        },
        new TextArgument("image url")
        {
            DefaultValue = new(null, "none"),
            Description = "The source link for the image."
        },
        new TextArgument("clickable url")
        {
            DefaultValue = new(null, "none"),
            Description = "Makes the title a clickable hyperlink."
        },
        new ReferenceArgument<Embed_CreateFieldMethod.DEmbedField>("fields")
        {
            DefaultValue = new(Array.Empty<Embed_CreateFieldMethod.DEmbedField>(), "none"),
            ConsumesRemainingValues = true,
            Description = $"List of fields, created using {NameOfMethod(typeof(Embed_CreateFieldMethod))}"
        }
    ];

    public override void Execute()
    {
        var embed = new DEmbed();

        if (Args.GetText("title") is { } title) embed["title"] = title;
        
        if (Args.GetText("description") is { } description) embed["description"] = description;
        
        if (Args.GetColor("color") is { a: > 0 } color)
        {
            int r = Mathf.RoundToInt(color.r * 255);
            int g = Mathf.RoundToInt(color.g * 255);
            int b = Mathf.RoundToInt(color.b * 255);
            
            embed["color"] = (r << 16) | (g << 8) | b;
        }

        if (Args.GetReference<Embed_CreateAuthorMethod.DEmbedAuthor>("author") is { } author) embed["author"] = author;
        
        if (Args.GetReference<Embed_CreateFooterMethod.DEmbedFooter>("footer") is { } footer) embed["footer"] = footer;
        
        if (Args.GetText("thumbnail url") is { } thumbnailUrl) embed["thumbnail"] = new JObject {{"url", thumbnailUrl}};
        
        if (Args.GetText("image url") is { } imageUrl) embed["image"] = new JObject {{"url", imageUrl}};

        if (Args.GetText("clickable url") is { } url) embed["url"] = url;

        var fields = Args.GetRemainingArguments<Embed_CreateFieldMethod.DEmbedField, ReferenceArgument<Embed_CreateFieldMethod.DEmbedField>>("fields");
        if (fields is { Length: > 0 })
        {
            embed["fields"] = JArray.FromObject(fields);
        }

        ReturnValue = embed;
    }
}