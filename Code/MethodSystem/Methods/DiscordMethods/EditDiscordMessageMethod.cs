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
public class EditDiscordMessageMethod : SynchronousMethod, ICanError
{
    public override string Description => "Edits a message sent by a discord webhook (with that same webhook).";

    public string[] ErrorReasons =>
    [
        ..HTTPPostMethod.HttpErrorReasons,
        "Message ID must not be empty.",
        "Provided URL is not a valid webhook URL."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("webhook url"),
        new TextArgument("message id")
        {
            Description = "You can get it by right-clicking on a message and clicking \"Copy message ID\""
        },
        new ReferenceArgument<DiscordMessageMethod.DMessage>("message object"),
        new TextArgument("thread id")
        {
            DefaultValue = new(string.Empty, "no thread")
        }
    ];

    public override void Execute()
    {
        var webhookUrl = Args.GetText("webhook url");
        var messageId = Args.GetText("message id");
        var messageObject = Args.GetReference<DiscordMessageMethod.DMessage>("message object");
        var threadId = Args.GetText("thread id");
        
        if (messageId.IsEmpty())
            throw new ScriptRuntimeError(this, ErrorReasons[^2]);
        
        if (!webhookUrl.StartsWith("https://discord.com/api/webhooks/"))
            throw new ScriptRuntimeError(this, ErrorReasons.Last());
        
        var messageURL = webhookUrl + "/messages/" + messageId +
                         (!threadId.IsEmpty() ? $"?thread_id={threadId}" : "");

        Timing.RunCoroutine(HTTPPostMethod.RequestSend(this, messageURL, messageObject, "PATCH"));
    }
}