using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.PlayerDataMethods;

[UsedImplicitly]
public class GetPlayerDataMethod : ReturningMethod, IAdditionalDescription, ICanError
{
    public override string Description => "Gets player data from the key.";

    public string AdditionalDescription => 
        "WARNING: This method will error if the key doesn't exist. " +
        $"Use {GetFriendlyName(typeof(HasPlayerDataMethod))} to verify if a key exists before calling this method.";
    
    public override TypeOfValue Returns => new UnknownTypeOfValue();

    public string[] ErrorReasons { get; } =
    [
        "Key was not found for the player."
    ];

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
            throw new ScriptRuntimeError(this, ErrorReasons[0]);
        }

        ReturnValue = value;
    }
}