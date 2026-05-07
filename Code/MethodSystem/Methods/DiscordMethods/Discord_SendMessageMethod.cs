using MEC;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.MethodSystem.Methods.HTTPMethods;

namespace SER.Code.MethodSystem.Methods.DiscordMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Discord_SendMessageMethod : SynchronousMethod, ICanError
{
    public override string Description => "Sends a message using a discord webhook.";

    public string[] ErrorReasons =>
    [
        ..HTTP_PostMethod.HttpErrorReasons,
        "Provided URL is not a valid webhook URL."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("webhook url"),
        new ReferenceArgument<Discord_CreateMessageMethod.DMessage>("message object"),
        new TextArgument("thread id")
        {
            DefaultValue = new(string.Empty, "no thread")
        }
    ];

    public override void Execute()
    {
        var webhookUrl = Args.GetText("webhook url");
        var messageObject = Args.GetReference<Discord_CreateMessageMethod.DMessage>("message object");
        var threadId = Args.GetText("thread id");
        
        if (!webhookUrl.StartsWith("https://discord.com/api/webhooks/"))
            throw new ScriptRuntimeError(this, ErrorReasons.Last());

        Timing.RunCoroutine(HTTP_PostMethod.RequestSend(
            this, 
            webhookUrl + (!threadId.IsEmpty() ? $"?thread_id={threadId}" : ""), 
            messageObject));
    }
}