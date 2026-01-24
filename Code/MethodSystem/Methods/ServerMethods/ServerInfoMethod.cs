using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.ServerMethods;

[UsedImplicitly]
public class ServerInfoMethod : ReturningMethod
{
    public override string Description => "Returns info about the server.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new OptionsArgument("info",
            "ip",
            "port",
            "name",
            "maxPlayers",
            "tps",
            "isVerified")
    ];

    public override TypeOfValue Returns => new TypesOfValue([
        typeof(TextValue),
        typeof(NumberValue),
        typeof(BoolValue)
    ]);

    public override void Execute()
    {
        ReturnValue = Args.GetOption("info") switch
        {
            "ip" => new TextValue(Server.IpAddress),
            "port" => new NumberValue(Server.Port),
            "name" => new TextValue(Server.ServerListName),
            "maxplayers" => new NumberValue(Server.MaxPlayers),
            "tps" => new NumberValue((decimal)Server.Tps),
            "isverified" => new BoolValue(CustomNetworkManager.IsVerified),
            _ => throw new TosoksFuckedUpException("out of order")
        };
    }
}