using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.HealthMethods;

[UsedImplicitly]
public class KillMethod : SynchronousMethod
{
    public override string Description => "Kills players.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new TextArgument("reason")
        {
            DefaultValue = new(string.Empty, "empty"),
        }
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        
        foreach (var player in players)
        {
            player.Kill();
        }
    }
}