using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.HealthMethods;

[UsedImplicitly]
public class SetHealthMethod : SynchronousMethod
{
    public override string Description => "Sets health for players.";
    public override Argument[] ExpectedArguments { get; } = 
    [
        new PlayersArgument("players"),
        new FloatArgument("health", 0)
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var health = Args.GetFloat("health");
        foreach (var player in players) player.Health = health;
    }
}