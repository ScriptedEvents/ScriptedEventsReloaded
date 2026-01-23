using MEC;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Yielding;
using SER.Code.MethodSystem.MethodDescriptors;
using UnityEngine.Networking;

namespace SER.Code.MethodSystem.Methods.HTTPMethods;

// ReSharper disable once InconsistentNaming
public class HTTPGetMethod : YieldingReferenceReturningMethod<JObject>, ICanError
{
    public override string Description =>
        "Sends a GET request to a provided URL and returns the response as a JSON object.";

    public string[] ErrorReasons =>
    [
        "Fetched value from the URL is not a valid JSON object.",
        nameof(UnityWebRequest.Result.ConnectionError),
        nameof(UnityWebRequest.Result.DataProcessingError),
        nameof(UnityWebRequest.Result.ProtocolError)
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("address")
    ];

    public override IEnumerator<float> Execute()
    {
        var address = Args.GetText("address");
        
        using UnityWebRequest webRequest = UnityWebRequest.Get(address);

        yield return Timing.WaitUntilDone(webRequest.SendWebRequest());

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            throw new ScriptRuntimeError(
                this, 
                $"Address {address} has returned {webRequest.result} ({webRequest.responseCode}): {webRequest.error}"
            );
        }

        try
        {
            ReturnValue = JObject.Parse(webRequest.downloadHandler.text);
        }
        catch (JsonReaderException)
        {
            throw new ScriptRuntimeError(this, ErrorReasons[0]);
        }
    }
}