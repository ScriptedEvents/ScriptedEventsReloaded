using MEC;
using Newtonsoft.Json.Linq;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Yielding;
using SER.Code.MethodSystem.Structures;
using UnityEngine.Networking;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

public record struct IPInfo(
    bool IsVPN = false,
    bool IsHosting = false,
    string Provider = "Unknown",
    string Country = "Unknown",
    string Type = "Unknown",
    int RiskScore = 0,
    int Confidence = 0,
    string FirstSeen = "Unknown",
    string LastSeen = "Unknown"
);

[UsedImplicitly]
public class GetIPInfoMethod : YieldingReferenceReturningMethod<IPInfo>, ICanError, IAdditionalDescription
{
    public override string Description => 
        "Fetches information about a provided player IP address using ProxyCheck.io (Keyless).";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayerArgument("player")
    ];

    public string AdditionalDescription =>
        $"The keyless API is limited to 100 queries per day, which may not be enough for a big server. " +
        $"Consider using {NameOfMethod(typeof(GetIPInfoWithKeyMethod))} with your own API key if you need more.";
    
    public string[] ErrorReasons { get; } =
    [
        "Failed to fetch IP info: %message%",
        "API Error: %message%"
    ];

    public override IEnumerator<float> Execute()
    {
        var ip = Args.GetPlayer("player").IpAddress;

        if (ip == "127.0.0.1" || ip == "localhost" || ip.StartsWith("192.168.") || ip.StartsWith("10.") || ip.StartsWith("172."))
        {
            ReturnValue = new IPInfo(Type: "Local Network");
            yield break;
        }

        // v3 keyless is limited to 100 queries per day
        string url = $"https://proxycheck.io/v3/{ip}";

        using UnityWebRequest webRequest = UnityWebRequest.Get(url);

        yield return Timing.WaitUntilDone(webRequest.SendWebRequest());

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            throw new ScriptRuntimeError(
                this, 
                ErrorReasons[0].Replace("%message%", webRequest.error ?? "Unknown error")
            );
        }

        JObject json = JObject.Parse(webRequest.downloadHandler.text);

        if (json["status"]?.ToString() == "error")
        {
            throw new ScriptRuntimeError(
                this, 
                ErrorReasons[1].Replace("%message%", json["message"]?.ToString() ?? "Unknown error")
            );
        }
        
        if (json[ip] is not { } data)
        {
             throw new ScriptRuntimeError(this, $"No data returned for IP: {ip}");
        }

        var detections = data["detections"];
        var network = data["network"];
        var location = data["location"];

        ReturnValue = new IPInfo(
            detections?["vpn"]?.Value<bool>() == true || detections?["proxy"]?.Value<bool>() == true,
            detections?["hosting"]?.Value<bool>() == true,
            network?["asn"]?.ToString() ?? network?["provider"]?.ToString() ?? "Unknown",
            location?["country_name"]?.ToString() ?? "Unknown",
            network?["type"]?.ToString() ?? "Unknown",
            detections?["risk"]?.Value<int>() ?? 0,
            detections?["confidence"]?.Value<int>() ?? 0,
            detections?["first_seen"]?.ToString() ?? "Unknown",
            detections?["last_seen"]?.ToString() ?? "Unknown"
        );
    }
}
