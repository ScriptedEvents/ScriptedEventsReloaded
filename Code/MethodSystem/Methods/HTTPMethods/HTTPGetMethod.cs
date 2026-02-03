using JetBrains.Annotations;
using MEC;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Yielding;
using SER.Code.MethodSystem.MethodDescriptors;
using UnityEngine.Networking;

namespace SER.Code.MethodSystem.Methods.HTTPMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class HTTPGetMethod : YieldingReferenceReturningMethod<JObject>, ICanError
{
    public override string Description =>
        "Sends a GET request to a provided URL and returns the response as a JSON object.";

    public string[] ErrorReasons => [
        ..HTTPPostMethod.HttpErrorReasons, 
        "Provided response was not a valid JSON object."
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
        
        if (webRequest.error is { } error)
        {
            throw new ScriptRuntimeError(this, 
                $"Address {address} has returned an error: {error}"
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