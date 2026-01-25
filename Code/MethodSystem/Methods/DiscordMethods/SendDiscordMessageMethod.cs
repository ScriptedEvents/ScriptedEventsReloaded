using JetBrains.Annotations;
using MEC;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.MethodSystem.Methods.HTTPMethods;

namespace SER.Code.MethodSystem.Methods.DiscordMethods;

[UsedImplicitly]
public class SendDiscordMessageMethod : SynchronousMethod, ICanError
{
    public override string Description => "Sends a message using a discord webhook.";

    public string[] ErrorReasons =>
    [
        ..HTTPPostMethod.HttpErrorReasons,
        "Provided URL is not a valid webhook URL."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("webhook url"),
        new ReferenceArgument<DiscordMessageMethod.DMessage>("message object")
    ];

    public override void Execute()
    {
        var webhookUrl = Args.GetText("webhook url");
        var messageObject = Args.GetReference<DiscordMessageMethod.DMessage>("message object");
        
        if (!webhookUrl.StartsWith("https://discord.com/api/webhooks/"))
            throw new ScriptRuntimeError(this, ErrorReasons.Last());

        Timing.RunCoroutine(HTTPPostMethod.RequestSend(this, webhookUrl, messageObject));
    }
}