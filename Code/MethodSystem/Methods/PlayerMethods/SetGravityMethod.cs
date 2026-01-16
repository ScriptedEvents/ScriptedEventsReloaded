using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class SetGravityMethod : SynchronousMethod
{
    public override string Description => "Changes player gravity.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new FloatArgument("gravity")
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var gravity = Args.GetFloat("gravity");
        
        players.ForEach(plr => plr.Gravity = new(0, -gravity, 0));
    }
}