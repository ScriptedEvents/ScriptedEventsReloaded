using Exiled.API.Enums;
using Exiled.API.Features;
using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class GiveEffectMethod : SynchronousMethod, IExiledMethod
{
    public override string Description => "Adds a provided effect to a player.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new EnumArgument<EffectType>("effect type"),
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
        var effectType = Args.GetEnum<EffectType>("effect type");
        var duration = (float)Args.GetDuration("duration").TotalSeconds;
        var intensity = (byte)Args.GetInt("intensity");
        
        players.ForEach(plr 
            => Player.Get(plr).EnableEffect(effectType, intensity, duration)
        );
    }
}