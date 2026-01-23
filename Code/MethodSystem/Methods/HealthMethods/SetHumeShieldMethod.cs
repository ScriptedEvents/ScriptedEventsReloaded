using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.HealthMethods;

[UsedImplicitly]
public class SetHumeShieldMethod : SynchronousMethod
{
    public override string Description => "Sets hume shield for players.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new FloatArgument("amount", 0)
        {
            Description = "The amount of hume shield to set."
        },
        new FloatArgument("limit", 0)
        {
            Description = "The maximal amount of hume shield."
        },
        new FloatArgument("regen rate", 0)
        {
            Description = "The rate of hume shield regenerated per second."
        },
        new DurationArgument("regen cooldown")
        {
            Description = "The cooldown before hume shield regeneration begins."
        }
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var amount = Args.GetFloat("amount");
        var limit = Args.GetFloat("limit");
        var regenRate = Args.GetFloat("regen rate");
        var regenCooldown = Args.GetDuration("regen cooldown");

        players.ForEach(p => 
        {
            p.HumeShield = amount;
            p.MaxHumeShield = limit;
            p.HumeShieldRegenRate = regenRate;
            p.HumeShieldRegenCooldown = (float)regenCooldown.TotalSeconds;
        });
    }
}