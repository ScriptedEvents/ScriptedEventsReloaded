using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers;
using SER.Code.MethodSystem.BaseMethods;

namespace SER.Code.MethodSystem.Methods.BroadcastMethods;

[UsedImplicitly]
public class ClearCountdownMethod : SynchronousMethod
{
    public override string Description => "Removes an active countdown for players if one is active.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players")
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        players.ForEach(plr =>
        {
            if (CountdownMethod.Coroutines.TryGetValue(plr, out var coroutine)) 
                coroutine.Kill();
        });
    }
}