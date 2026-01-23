using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using Utils;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class ExplodeMethod : SynchronousMethod
{
    public override string Description => "Explodes players.";
    
    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players")
    ];

    public override void Execute()
    {
        Args.GetPlayers("players").ForEach(player => 
            ExplosionUtils.ServerExplode(player.ReferenceHub, ExplosionType.Grenade));
    }
}