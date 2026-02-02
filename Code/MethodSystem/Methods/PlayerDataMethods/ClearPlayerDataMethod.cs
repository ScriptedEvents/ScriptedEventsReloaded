using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.PlayerDataMethods;

[UsedImplicitly]
public class ClearPlayerDataMethod : SynchronousMethod
{
    public override string Description => "Clears data associated with specified players";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new TextArgument("key to clear")
        {
            DefaultValue = new(string.Empty, "all keys"),
            Description = "Don't provide this argument to clear all keys for players, provide if you want to clear specific key"
        }
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var key = Args.GetText("key to clear");
        
        Action<Dictionary<string, Value>> func = key.IsEmpty()
            ? dict => dict.Clear()
            : dict => dict.Remove(key);
        
        players.ForEach(p =>
        {
            if (SetPlayerDataMethod.PlayerData.TryGetValue(p, out var dict))
            {
                func(dict);
            }
        });
    }
}