using PlayerStatsSystem;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.HealthMethods;

// ReSharper disable once InconsistentNaming
[UsedImplicitly]
public class SetAHPMethod : SynchronousMethod
{
    public override string Description => "Adds or removes AHP for players.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new FloatArgument("amount")
        {
            Description = "Adds this much AHP. Use a negative amount to remove AHP immediately; the other settings apply only when adding AHP."
        },
        new FloatArgument("limit", 0)
        {
            DefaultValue = new(75, null),
            Description = "The upper limit of AHP."
        },
        new FloatArgument("decay")
        {
            DefaultValue = new(1.2, null),
            Description = "AHP lost per second. Use a negative number to regenerate AHP."
        },
        new FloatArgument("efficacy", 0, 1, true)
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
            var stat = plr.ReferenceHub.playerStats.GetModule<AhpStat>();
            if (amount < 0)
            {
                RemoveAhp(stat, -amount);
            }
            else
            {
                stat.ServerAddProcess(amount, limit, decay, efficacy, (float)sustain.TotalSeconds, isPersistent);
            }
        }
    }

    internal static void RemoveAhp(AhpStat stat, float amount)
    {
        foreach (var snapshot in stat.GenerateSnapshots().ToArray())
        {
            if (amount <= 0) break;
            if (!stat.ServerTryGetProcess(snapshot.KillCode, out var process)) continue;

            var removed = Math.Min(amount, Math.Max(0, process.CurrentAmount));
            process.CurrentAmount -= removed;
            amount -= removed;
            if (process.CurrentAmount == 0 && !process.Persistant)
                stat.ServerKillProcess(process.KillCode);
        }

        // ServerUpdateProcesses also advances decay and sustain. Refresh the synced
        // total directly so removing AHP does not advance those timers a second time.
        var remaining = stat.GenerateSnapshots().ToArray();
        var limit = remaining.Aggregate(stat.MaxValue, (maximum, process) => Math.Max(maximum, process.Limit));
        stat.CurValue = Math.Min(limit, Math.Max(0, remaining.Sum(process => process.CurrentAmount)));
    }
}
