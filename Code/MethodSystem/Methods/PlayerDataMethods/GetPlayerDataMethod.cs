using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.PlayerDataMethods;

[UsedImplicitly]
public class GetPlayerDataMethod : ReturningMethod
{
    public override string Description => "Gets player data from the key.";

    public override TypeOfValue Returns => new UnknownTypeOfValue();
    
    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayerArgument("player"),
        new TextArgument("key")
    ];

    public override void Execute()
    {
        var player = Args.GetPlayer("player");
        var key = Args.GetText("key");

        if (!SetPlayerDataMethod.PlayerData.TryGetValue(player, out var dict) || 
            !dict.TryGetValue(key, out var value))
        {
            throw new ScriptRuntimeError(this, $"Key '{key}' was not found for player '{player.Nickname}'.");
        }

        ReturnValue = value;
    }
}