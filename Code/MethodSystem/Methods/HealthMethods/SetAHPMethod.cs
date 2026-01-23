using PlayerStatsSystem;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.HealthMethods;

// ReSharper disable once InconsistentNaming
public class SetAHPMethod : SynchronousMethod
{
    public override string Description => "Sets the amount of AHP for players.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new FloatArgument("amount", 0)
        {
            Description = "The amount of artificial health to ADD to the player. Use a negative number to remove."
        },
        new FloatArgument("limit", 0)
        {
            DefaultValue = new(75, null),
            Description = "The upper limit of AHP."
        },
        new FloatArgument("decay", 0)
        {
            DefaultValue = new(1.2, null),
            Description = "How much AHP is lost per second."
        },
        new FloatArgument("efficacy", 0, 1)
        {
            DefaultValue = new(0.7, "70%"),
            Description = "The percent of incoming damage absorbed by AHP."
        },
        new DurationArgument("sustain")
        {
            DefaultValue = new(TimeSpan.Zero, "instant (0 seconds)"),
            Description = "The amount of time before the AHP begins to decay."
        },
        new BoolArgument("isPersistent")
        {
            DefaultValue = new(false, null),
            Description = "Whether or not the AHP process stays after AHP reaches 0."
        }
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var amount = Args.GetFloat("amount");
        var limit = Args.GetFloat("limit");
        var decay = Args.GetFloat("decay");
        var efficacy = Args.GetFloat("efficacy");
        var sustain = Args.GetDuration("sustain");
        var isPersistent = Args.GetBool("isPersistent");

        foreach (var plr in players)
        {
            plr.ReferenceHub.playerStats.GetModule<AhpStat>()
                .ServerAddProcess(amount, limit, decay, efficacy, (float)sustain.TotalSeconds, isPersistent);
        }
    }
}