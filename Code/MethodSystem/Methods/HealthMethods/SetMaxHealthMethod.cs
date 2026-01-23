using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.HealthMethods;

[UsedImplicitly]
public class SetMaxHealthMethod : SynchronousMethod
{
    public override string Description => "Sets the max health of players.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new FloatArgument("maxHealth", 1)
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var maxHealth = Args.GetFloat("maxHealth");
        
        foreach (var player in players) player.MaxHealth = maxHealth;
    }
}