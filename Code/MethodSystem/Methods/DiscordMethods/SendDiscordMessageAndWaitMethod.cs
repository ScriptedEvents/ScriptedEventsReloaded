using JetBrains.Annotations;
using MEC;
using Newtonsoft.Json.Linq;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Yielding;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.MethodSystem.Methods.HTTPMethods;
using SER.Code.ValueSystem;
using UnityEngine.Networking;

namespace SER.Code.MethodSystem.Methods.DiscordMethods;

[UsedImplicitly]
public class SendDiscordMessageAndWaitMethod : YieldingReturningMethod<TextValue>, ICanError
{
    public override string Description => 
        "Sends a message using a discord webhook and waits until it is completed. Returns the message id.";

    public string[] ErrorReasons =>
    [
        ..HTTPPostMethod.HttpErrorReasons,
        "Provided URL is not a valid webhook URL."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("webhook url"),
        new ReferenceArgument<DiscordMessageMethod.DMessage>("message object"),
        new TextArgument("thread id")
        {
            DefaultValue = new(string.Empty, "no thread")
        }
    ];

    public override IEnumerator<float> Execute()
    {
        var webhookUrl = Args.GetText("webhook url");
        var messageObject = Args.GetReference<DiscordMessageMethod.DMessage>("message object");
        var threadId = Args.GetText("thread id");
        
        if (!webhookUrl.StartsWith("https://discord.com/api/webhooks/"))
            throw new ScriptRuntimeError(this, ErrorReasons.Last());
        
        using UnityWebRequest request = new UnityWebRequest(
            webhookUrl + "?wait=true" + (!threadId.IsEmpty() ? $"&thread_id={threadId}" : ""), 
            "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(messageObject.ToString());
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return Timing.WaitUntilDone(request.SendWebRequest());

        if (request.error is { } error)
        {
            throw new ScriptRuntimeError(
                this,
                $"Address {webhookUrl} has returned an error: {error}"
            );
        }

        try
        {
            ReturnValue = new StaticTextValue(
                JObject.Parse(request.downloadHandler.text)["id"]?.Value<string>() ??
                throw new NotOurFaultException("Excuse me? WHERE'S THE ID")
            );
        }
        catch
        {
            throw new ScriptRuntimeError(this, "Returned message object from discord is invalid.");
        }
    }
}