using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class SetSizeMethod : SynchronousMethod
{
    public override string Description => "Sets the size of players.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new FloatArgument("x size"),
        new FloatArgument("y size"),
        new FloatArgument("z size")
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var x = Args.GetFloat("x size");
        var y = Args.GetFloat("y size");
        var z = Args.GetFloat("z size");
        
        players.ForEach(plr =>
        {
            plr.Scale = new(x, y, z);
        });
    }
}