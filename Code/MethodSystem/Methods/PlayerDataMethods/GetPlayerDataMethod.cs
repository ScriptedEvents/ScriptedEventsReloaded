using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;
using SER.Code.ValueSystem.Other;

namespace SER.Code.MethodSystem.Methods.PlayerDataMethods;

[UsedImplicitly]
public class GetPlayerDataMethod : ReturningMethod, IAdditionalDescription, ICanError
{
    public override string Description => "Gets player data from the key.";

    public string AdditionalDescription => 
        "It's recommended to set 'default value' argument for you to not get errors when the key doesn't exist " +
        "for a given player!";
    
    public override TypeOfValue Returns => new UnknownTypeOfValue();

    public string[] ErrorReasons { get; } =
    [
        "Key %key% was not found for the player %player_name%."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayerArgument("player"),
        new TextArgument("key"),
        new AnyValueArgument("default value")
        {
            Description = "If the key doesn't exist, this value will be returned.",
            DefaultValue = new(null, "no default value - will error when key doesnt exist")
        }
    ];

    public override void Execute()
    {
        var player = Args.GetPlayer("player");
        var key = Args.GetText("key");

        if (SetPlayerDataMethod.PlayerData.TryGetValue(player, out var dict) &&
            dict.TryGetValue(key, out var value))
        {
            ReturnValue = value;
        }
        else if (Args.GetAnyValue("default value") is { } defaultValue)
        {
            ReturnValue = defaultValue;
        }
        else
        {
            throw new ScriptRuntimeError(
                this,
                ErrorReasons[0].Replace("%key%", key).Replace("%player_name%", player.Nickname)
            );
        }
    }
}