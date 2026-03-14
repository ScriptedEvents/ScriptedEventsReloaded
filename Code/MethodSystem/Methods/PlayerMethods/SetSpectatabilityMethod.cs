using JetBrains.Annotations;
using PlayerRoles.Spectating;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class SetSpectatabilityMethod : SynchronousMethod
{
    public override string Description => "Sets spectatability for players.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new BoolArgument("is spectatable")
    ];

    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var isSpectatable = Args.GetBool("is spectatable");
        
        players.ForEach(player =>
        {
            SpectatableVisibilityManager.SetHidden(player.ReferenceHub, isSpectatable);
        });
    }
}