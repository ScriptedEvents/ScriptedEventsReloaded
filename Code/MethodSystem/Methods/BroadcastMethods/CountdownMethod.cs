using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using MEC;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.BroadcastMethods;

[UsedImplicitly]
public class CountdownMethod : SynchronousMethod
{
    public static readonly Dictionary<Player, CoroutineHandle> Coroutines = new();
    
    public override string Description => "Creates a countdown using broadcasts.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new DurationArgument("duration"),
        new TextArgument("title")
        {
            Description = "Use %seconds% to get seconds remaining until the end of the countdown."
        }
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var duration = Args.GetDuration("duration");
        var title = Args.GetText("title");
        
        foreach (var plr in players)
        {
            var coro = RunCoroutine(Countdown(plr, duration, title));
            if (Coroutines.TryGetValue(plr, out var coroutine)) 
                coroutine.Kill();
            
            Coroutines[plr] = coro;
        }
    }

    private static IEnumerator<float> Countdown(Player player, TimeSpan duration, string title)
    {
        while (duration.TotalSeconds > 0)
        {
            var isLastCycle = duration.TotalSeconds <= 1;
            var secondsRemaining = Math.Round(duration.TotalSeconds, MidpointRounding.AwayFromZero);
            var currentTitle = title.Replace("%seconds%", secondsRemaining.ToString());
            
            player.ClearBroadcasts();
            Server.SendBroadcast(player, currentTitle, (ushort)(isLastCycle ? 1 : 2));
            
            duration -= TimeSpan.FromSeconds(1);
            yield return Timing.WaitForSeconds(1);
        }
    }
}