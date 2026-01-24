using JetBrains.Annotations;
using MEC;
using Newtonsoft.Json.Linq;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.MethodSystem.Methods.HTTPMethods;

namespace SER.Code.MethodSystem.Methods.DiscordMethods;

[UsedImplicitly]
public class SendDiscordMessageMethod : SynchronousMethod, ICanError
{
    public override string Description => "Sends a message using a discord webhook.";

    public string[] ErrorReasons => HTTPPostMethod.HttpErrorReasons;

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("webhook url"),
        new TextArgument("message content"),
        new TextArgument("webhook name")
        {
            DefaultValue = new(null, "default")
        },
        new TextArgument("avatar url")
        {
            DefaultValue = new(null, "default")
        }
    ];

    public override void Execute()
    {
        var webhookUrl = Args.GetText("webhook url");
        var messageContent = Args.GetText("message content");
        var webhookName = Args.GetText("webhook name").MaybeNull();
        var avatarUrl = Args.GetText("avatar url").MaybeNull();
        
        JObject json = new()
        {
            ["content"] = messageContent
        };

        if (webhookName != null) json["username"] = webhookName;
        if (avatarUrl != null) json["avatar_url"] = avatarUrl;

        Timing.RunCoroutine(HTTPPostMethod.SendPost(this, webhookUrl, json.ToString()));
    }
}