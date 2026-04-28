using System.Reflection;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.EffectMethods;

[UsedImplicitly]
public class GiveEffectMethod : SynchronousMethod
{
    private static readonly MethodInfo EnableEffectMethod = 
        typeof(Player).GetMethod("EnableEffect", [typeof(byte), typeof(float), typeof(bool)])
        ?? throw new Exception("Could not find EnableEffect method for Player");

    public override string Description => "Adds a provided effect for specified players.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new EffectTypeArgument("effect"),
        new DurationArgument("duration")
        {
            DefaultValue = new(TimeSpan.Zero, "infinite")
        },
        new IntArgument("intensity", 0, 255)
        {
            DefaultValue = new(1, null)
        },
        new BoolArgument("add duration if active")
        {
            DefaultValue = new(false, null)
        }
    ];

    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var effectType = Args.GetEffectType("effect");
        var duration = (float)Args.GetDuration("duration").TotalSeconds;
        var intensity = (byte)Args.GetInt("intensity");
        var addDurationIfActive = Args.GetBool("add duration if active");
        
        var method = EnableEffectMethod.MakeGenericMethod(effectType);
        
        foreach (var plr in players)
        {
            method.Invoke(plr, [intensity, duration, addDurationIfActive]);
        }
    }
}