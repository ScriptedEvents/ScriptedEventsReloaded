using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;

namespace SER.Code.MethodSystem.Methods.HealthMethods;

[UsedImplicitly]
public class AddHumeMethod : SynchronousMethod
{
    public override string Description => $"Adds hume shield to players. Do not confuse this method with {nameof(SetHumeShieldMethod)}";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new FloatArgument("amount", 0)
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var amount = Args.GetFloat("amount");
        
        players.ForEach(plr =>
        {
            plr.HumeShield = Math.Min(plr.MaxHumeShield, plr.HumeShield + amount);
        });
    }
}