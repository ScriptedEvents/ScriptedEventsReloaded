using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.PlayerDataMethods;

[UsedImplicitly]
public class HasPlayerDataMethod : ReturningMethod<BoolValue>
{
    public override string Description => "Checks if a given key has an associated value for a given player.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayerArgument("player"),
        new TextArgument("key")
    ];
    
    public override void Execute()
    {
        var player = Args.GetPlayer("player");
        var key = Args.GetText("key");
        
        ReturnValue = new BoolValue(
            SetPlayerDataMethod.PlayerData.TryGetValue(player, out var dict) 
            && dict.ContainsKey(key)
        );
    }
}