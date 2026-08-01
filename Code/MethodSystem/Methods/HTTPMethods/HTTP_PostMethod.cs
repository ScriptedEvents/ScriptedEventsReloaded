using MEC;
using Newtonsoft.Json.Linq;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.MethodSystem.BaseMethods.Yielding;
using SER.Code.MethodSystem.Structures;
using SER.Code.Plugin;
using UnityEngine.Networking;

namespace SER.Code.MethodSystem.Methods.HTTPMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class HTTP_PostMethod : YieldingMethod, ICanError
{
    public override string Description => "Sends a POST request to a provided URL.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("address"),
        new ReferenceArgument<JObject>("json data to post")
    ];
    
    public static string[] HttpErrorReasons { get; } =
    [
        nameof(UnityWebRequest.Result.ConnectionError),
        nameof(UnityWebRequest.Result.DataProcessingError),
        nameof(UnityWebRequest.Result.ProtocolError),
        "Response exceeded the configured maximum size."
    ];

    public string[] ErrorReasons => HttpErrorReasons;

    public override IEnumerator<float> Execute()
    {
        var address = Args.GetText("address");
        var jsonData = Args.GetReference<JObject>("json data to post");
        
        return RequestSend(this, address, jsonData);
    }
    
    public static IEnumerator<float> RequestSend(Method caller, string url, JObject? jsonData, string method = "POST")
    {
        using var request = new UnityWebRequest(url, method);

        if (jsonData is not null)
        {
            var bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData.ToString());
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.SetRequestHeader("Content-Type", "application/json");   
        }

        using var wait = SendWithPolicy(caller, request);
        while (wait.MoveNext()) yield return wait.Current;
        
        if (request.error is { } error)
        {
            throw new ScriptRuntimeError(
                caller,
                $"Address {url} has returned an error: {error}"
            );
        }
    }

    public static IEnumerator<float> SendWithPolicy(Method caller, UnityWebRequest request)
    {
        request.timeout = Math.Max(MainPlugin.Instance.Config.NetworkRequestTimeoutSeconds, 1);
        var maxResponseBytes = Math.Max(MainPlugin.Instance.Config.MaxNetworkResponseBytes, 1);
        var operation = request.SendWebRequest();

        while (!operation.isDone)
        {
            if (request.downloadedBytes > (ulong)maxResponseBytes)
            {
                request.Abort();
                throw new ScriptRuntimeError(
                    caller,
                    $"Response exceeded the configured maximum size of {maxResponseBytes} bytes."
                );
            }

            yield return Timing.WaitForOneFrame;
        }

        if (request.downloadedBytes > (ulong)maxResponseBytes)
        {
            throw new ScriptRuntimeError(
                caller,
                $"Response exceeded the configured maximum size of {maxResponseBytes} bytes."
            );
        }
    }
}
