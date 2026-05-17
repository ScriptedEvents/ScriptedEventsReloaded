using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;

namespace SER.Code.MethodSystem.Methods.PlayerDataMethods;

[UsedImplicitly]
public class GetPlayerDataMethod : ReturningMethod, IAdditionalDescription
{
    public override string Description => "Gets player data from the key.";

    public string AdditionalDescription => "If the key does not exist, invalid value will be returned.";
    
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

        if (SetPlayerDataMethod.PlayerData.TryGetValue(player, out var dict) &&
            dict.TryGetValue(key, out var value))
        {
            ReturnValue = value;
        }
        else
        {
            ReturnValue = new InvalidValue();
        }
    }
}