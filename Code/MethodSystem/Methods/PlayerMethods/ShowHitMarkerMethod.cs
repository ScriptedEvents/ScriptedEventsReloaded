using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class ShowHitMarkerMethod : SynchronousMethod
{
    public override string Description =>
        "Shows a hit marker to players.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new FloatArgument("hitmarker size") 
            { DefaultValue = new(1f, "Default Size") },
        new BoolArgument("should play audio")
            { DefaultValue = new(true, "Hitmarker will make a noise.")},
        new EnumArgument<HitmarkerType>("hitmarker type")
            { DefaultValue = new(HitmarkerType.Regular, "Regular Hitmarker")}
    ];

    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var size = Args.GetFloat("hitmarker size");
        var playAudio = Args.GetBool("should play audio");
        var hitmarkerType = Args.GetEnum<HitmarkerType>("hitmarker type");
        
        players.ForEach(plr => Hitmarker.SendHitmarkerDirectly(plr.ReferenceHub, size, playAudio, hitmarkerType));
    }
}