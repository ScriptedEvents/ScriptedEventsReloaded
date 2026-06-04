using MEC;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Methods.HTTPMethods;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.DiscordMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Discord_EditMessageMethod : SynchronousMethod, ICanError
{
    public override string Description => "Edits a message sent by a discord webhook (with that same webhook).";

    public string[] ErrorReasons =>
    [
        ..HTTP_PostMethod.HttpErrorReasons,
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
        new ReferenceArgument<Discord_CreateMessageMethod.DMessage>("message object"),
        new TextArgument("thread id")
        {
            DefaultValue = new(string.Empty, "no thread")
        }
    ];

    public override void Execute()
    {
        var webhookUrl = Args.GetText("webhook url");
        var messageId = Args.GetText("message id");
        var messageObject = Args.GetReference<Discord_CreateMessageMethod.DMessage>("message object");
        var threadId = Args.GetText("thread id");
        
        if (messageId.IsEmpty())
            throw new ScriptRuntimeError(this, ErrorReasons[^2]);
        
        if (!webhookUrl.StartsWith("https://discord.com/api/webhooks/"))
            throw new ScriptRuntimeError(this, ErrorReasons.Last());
        
        var messageURL = webhookUrl + "/messages/" + messageId +
                         (!threadId.IsEmpty() ? $"?thread_id={threadId}" : "");

        Timing.RunCoroutine(HTTP_PostMethod.RequestSend(this, messageURL, messageObject, "PATCH"));
    }
}