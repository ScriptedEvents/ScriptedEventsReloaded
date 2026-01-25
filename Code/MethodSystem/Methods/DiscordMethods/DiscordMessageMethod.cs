using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;

namespace SER.Code.MethodSystem.Methods.DiscordMethods;

[UsedImplicitly]
public class DiscordMessageMethod : ReferenceReturningMethod<DiscordMessageMethod.DMessage>, IAdditionalDescription, ICanError
{
    public class DMessage : JObject;
    
    public override string Description => "Creates a discord message object.";

    public string AdditionalDescription =>
        $"This method does NOT send the message. Use {GetFriendlyName(typeof(SendDiscordMessageMethod))} for that.";

    public string[] ErrorReasons { get; } =
    [
        "Provided more than 10 embeds. Discord only allows 10 embeds per message."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("message content")
        {
            Description = "The main message text (up to 2000 characters)",
            DefaultValue = new(null, "empty")
        },
        new TextArgument("sender name")
        {
            Description = "Overrides the webhook's default display name.",
            DefaultValue = new(null, "default")
        },
        new TextArgument("sender avatar url")
        {
            Description = "Overrides the webhook's default profile picture.",
            DefaultValue = new(null, "default")
        },
        new ReferenceArgument<DiscordEmbedMethod.DEmbed>("embeds")
        {
            Description = "An list containing up to 10 rich embed objects.",
            // i dont know if we can use both at the same time lmao - andrzej
            DefaultValue = new(Array.Empty<DiscordEmbedMethod.DEmbed>(), "none"),
            ConsumesRemainingValues = true
        }
    ];

    public override void Execute()
    {
        var messageContent = Args.GetText("message content").MaybeNull();
        var webhookName = Args.GetText("sender name").MaybeNull();
        var avatarUrl = Args.GetText("sender avatar url").MaybeNull();
        var embeds = Args.GetRemainingArguments<DiscordEmbedMethod.DEmbed, ReferenceArgument<DiscordEmbedMethod.DEmbed>>("embeds");

        DMessage json = new();

        if (messageContent != null) json["content"] = messageContent;
        if (webhookName != null) json["username"] = webhookName;
        if (avatarUrl != null) json["avatar_url"] = avatarUrl;
        if (embeds.Any())
        {
            if (embeds.Length > 10)
            {
                throw new ScriptRuntimeError(this, ErrorReasons[0]);
            }
            
            json["embeds"] = JArray.FromObject(embeds);
        }

        ReturnValue = json;
    }
}