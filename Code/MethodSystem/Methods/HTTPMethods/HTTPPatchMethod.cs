using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using MEC;
using Newtonsoft.Json.Linq;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using UnityEngine.Networking;

namespace SER.Code.MethodSystem.Methods.HTTPMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class HTTPPatchMethod : SynchronousMethod, ICanError
{
    public override string Description => "Sends a PATCH request to a provided URL.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("address"),
        new ReferenceArgument<JObject>("json data to patch")
    ];

    public string[] ErrorReasons =>
    [
        nameof(UnityWebRequest.Result.ConnectionError),
        nameof(UnityWebRequest.Result.DataProcessingError),
        nameof(UnityWebRequest.Result.ProtocolError)
    ];
    
    public override void Execute()
    {
        var address = Args.GetText("address");
        var jsonData = Args.GetReference<JObject>("json data to patch");
        Timing.RunCoroutine(HTTPPostMethod.RequestSend(this, address, jsonData, "PATCH"));
    }
}