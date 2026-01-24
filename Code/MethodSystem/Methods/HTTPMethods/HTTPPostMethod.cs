using JetBrains.Annotations;
using MEC;
using Newtonsoft.Json.Linq;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using UnityEngine.Networking;

namespace SER.Code.MethodSystem.Methods.HTTPMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class HTTPPostMethod : SynchronousMethod, ICanError
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
        nameof(UnityWebRequest.Result.ProtocolError)
    ];

    public string[] ErrorReasons => HttpErrorReasons;

    public override void Execute()
    {
        var address = Args.GetText("address");
        var jsonData = Args.GetReference<JObject>("json data to post");
        
        Timing.RunCoroutine(SendPost(this, address, jsonData.ToString()));
    }
    
    public static IEnumerator<float> SendPost(Method caller, string url, string jsonData)
    {
        using UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return Timing.WaitUntilDone(request.SendWebRequest());

        Log.Signal($"{request.error} error");
        if (request.result != UnityWebRequest.Result.Success)
        {
            throw new ScriptRuntimeError(
                caller, 
                $"Address {url} has returned {request.result} ({request.responseCode}): {request.error}"
            );
        }
    }
}