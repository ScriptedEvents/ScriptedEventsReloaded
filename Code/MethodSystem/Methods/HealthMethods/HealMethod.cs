using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;

namespace SER.Code.MethodSystem.Methods.HealthMethods;

[UsedImplicitly]
public class HealMethod : SynchronousMethod
{
    public override string Description => "Heals players. Doesn't exceed their max health.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players to heal"),
        new FloatArgument("amount", 0)
        {
            DefaultValue = new(float.MaxValue, "max amount"),
            Description = "If this argument is not provided, all players will be fully healed."
        }  
    ];
    
    public override void Execute()
    {
        var amount = Args.GetFloat("amount");
        Args.GetPlayers("players to heal").ForEach(plr =>
        {
            plr.Heal(amount);
        });
    }
}