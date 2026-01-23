using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.HealthMethods;

[UsedImplicitly]
public class DamageMethod : SynchronousMethod
{
    public override string Description => "Damages players.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new FloatArgument("amount", 0),
        new TextArgument("reason")
        {
            DefaultValue = new(string.Empty, "empty")
        }
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var amount = Args.GetFloat("amount");
        var reason = Args.GetText("reason");
        
        foreach (var plr in players)
        {
            plr.Damage(amount, reason);
        }
    }
}