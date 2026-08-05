using System.Net;
using Newtonsoft.Json;
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
public class GetIPInfoMethod : YieldingReferenceReturningMethod<IPInfo>, ICanError, IAdditionalDescription, IHasAliases
{
    public override string Description => 
        "Fetches information about a provided player IP address using ProxyCheck.io.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayerArgument("player"),
        new TextArgument("apiKey")
        {
            Description = "The API key to use for the request. If not provided, the keyless API will be used.",
            DefaultValue = new Argument.Default(null, "Keyless")
        }
    ];
    
    public string[] Aliases => ["GetIPInfoWithKey"];

    public string AdditionalDescription =>
        "The keyless API is limited to 100 queries per day, which may not be enough for a big server. " +
        "Consider providing your own API key from https://proxycheck.io/ if you need more " +
        "(up to 1,000 requests per day on the free tier).";

    public string[] ErrorReasons { get; } =
    [
        "Failed to fetch IP info: %message%",
        "API Error: %message%"
    ];

    public override IEnumerator<float> Execute()
    {
        var ip = Args.GetPlayer("player").IpAddress;

        if (IsLocalNetworkAddress(ip))
        {
            ReturnValue = new IPInfo(Type: "Local Network");
            yield break;
        }

        var key = Args.GetText("apiKey");

        // v3 keyless is limited to 100 queries per day
        string url = string.IsNullOrEmpty(key) 
            ? $"https://proxycheck.io/v3/{ip}" 
            : $"https://proxycheck.io/v3/{ip}?key={key}";

        using UnityWebRequest webRequest = UnityWebRequest.Get(url);

        using var wait = HTTPMethods.HTTP_PostMethod.SendWithPolicy(this, webRequest);
        while (wait.MoveNext()) yield return wait.Current;

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            throw new ScriptRuntimeError(
                this, 
                ErrorReasons[0].Replace("%message%", webRequest.error ?? "Unknown error")
            );
        }

        JObject json;
        try
        {
            json = JObject.Parse(webRequest.downloadHandler.text);
        }
        catch (JsonReaderException exception)
        {
            throw new ScriptRuntimeError(this, $"Failed to parse IP info response: {exception.Message}");
        }

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

    private static bool IsLocalNetworkAddress(string ip)
    {
        if (string.Equals(ip, "localhost", StringComparison.OrdinalIgnoreCase))
            return true;

        if (!IPAddress.TryParse(ip, out var address))
            return false;

        if (IPAddress.IsLoopback(address))
            return true;

        var bytes = address.GetAddressBytes();
        return bytes.Length == 4 &&
               (bytes[0] == 10 ||
                bytes[0] == 192 && bytes[1] == 168 ||
                bytes[0] == 172 && bytes[1] is >= 16 and <= 31);
    }
}
