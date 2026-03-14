using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class JumpMethod : SynchronousMethod, IAdditionalDescription
{
    public override string Description =>
        "Makes players jump (with modifiable jump strength).";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new FloatArgument("jump strength") 
            { DefaultValue = new(4.9f, "default jump strength") }
    ];

    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var jumpStrength = Args.GetFloat("jump strength");
        
        players.ForEach(plr => plr.Jump(jumpStrength));
    }

    public string AdditionalDescription => "This also works for players in the air. Allowing for mid-air jumps.";
}