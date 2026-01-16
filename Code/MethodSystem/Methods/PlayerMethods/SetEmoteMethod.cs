using JetBrains.Annotations;
using PlayerRoles.FirstPersonControl.Thirdperson.Subcontrollers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class SetEmoteMethod : SynchronousMethod
{
    public override string Description => "Sets emotion for specified players";

    public override Argument[] ExpectedArguments { get; } = 
    [
        new PlayersArgument("players"),
        new EnumArgument<EmotionPresetType>("emotion")
    ];

    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        EmotionPresetType emotion = Args.GetEnum<EmotionPresetType>("emotion");
        
        players.ForEach(p => p.Emotion = emotion);
    }
}
