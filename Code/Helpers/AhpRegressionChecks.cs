using System.Reflection;
using PlayerStatsSystem;
using SER.Code.MethodSystem.Methods.HealthMethods;

namespace SER.Code.Helpers;

internal static class AhpRegressionChecks
{
    internal static string? Verify()
    {
        // Seed real game processes without ServerAddProcess, which requires Unity's clock.
        var stat = new AhpStat { MaxValue = 75 };
        var processes = (List<AhpProcess>)typeof(AhpStat)
            .GetField("_activeProcesses", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stat)!;
        var first = new AhpProcess(20, 75, 0, 0.7f, 5, false);
        var second = new AhpProcess(30, 75, -2, 0.7f, 10, true);
        processes.Add(first);
        processes.Add(second);

        SetAHPMethod.RemoveAhp(stat, 25);
        if (stat.CurValue != 25 || processes.Count != 1 || second.CurrentAmount != 25
            || second.DecayRate != -2 || second.SustainTime != 10 || second.Limit != 75)
            return "Removing AHP did not subtract across existing processes or changed their settings.";

        // A negative process would increase this hit's damage from 3 to 9.
        var damage = stat.ServerProcessDamage(10);
        if (Math.Abs(damage - 3) > 0.001f || second.CurrentAmount != 18)
            return "Removing AHP left a negative process that changed incoming damage.";

        SetAHPMethod.RemoveAhp(stat, 100);
        if (stat.CurValue != 0 || second.CurrentAmount != 0 || processes.Count != 1)
            return "Removing more AHP than available did not keep the total at zero or lost a persistent process.";

        var ordinary = new AhpProcess(15, 75, 1.2f, 0.7f, 0, false);
        processes.Add(ordinary);
        SetAHPMethod.RemoveAhp(stat, 15);
        if (stat.CurValue != 0 || processes.Contains(ordinary))
            return "Removing all AHP left a nonpersistent process active.";

        SetAHPMethod.RemoveAhp(stat, 5);
        if (stat.CurValue != 0 || processes.Any(process => process.CurrentAmount < 0))
            return "Removing AHP from an empty total created a negative amount.";

        processes.Clear();
        processes.Add(new AhpProcess(75, 100, -2, 0.7f, 5, false));
        processes.Add(new AhpProcess(75, 100, -2, 0.7f, 5, false));
        SetAHPMethod.RemoveAhp(stat, 10);
        return stat.CurValue == 100 && processes.Sum(process => process.CurrentAmount) == 140
            ? null : "Removing AHP did not respect the existing processes' combined limit.";
    }
}
