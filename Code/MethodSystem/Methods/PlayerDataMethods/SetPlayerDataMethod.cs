using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.PlayerDataMethods;

// ReSharper disable once ClassNeverInstantiated.Global
public class SetPlayerDataMethod : SynchronousMethod, IAdditionalDescription
{
    public static readonly Dictionary<Player, Dictionary<string, Value>> PlayerData = [];
    
    public override string Description => "Associates a custom key with a value for a given player.";
    
    public string AdditionalDescription =>
        "This method ties a specific value to a specific player, allowing you to e.g. keep track of how many seconds " +
        "each player was on surface zone, as you're tying the amount of time to a specific player, independently of " +
        "other players. For this, you would create a key e.g. 'surfaceTime', and under that key you can start saving " +
        "the value. It's basically a dictionary/hashmap attached to a player.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayerArgument("player"),
        new TextArgument("key"),
        new AnyValueArgument("value to set")
    ];

    public override void Execute()
    {
        var player = Args.GetPlayer("player");
        var key = Args.GetText("key");
        var valueToSet = Args.GetAnyValue("value to set");
        
        if (PlayerData.TryGetValue(player, out var dict))
        {
            dict[key] = valueToSet;
        }
        else
        {
            PlayerData.Add(player, new Dictionary<string, Value> { { key, valueToSet } });
        }
    }
}