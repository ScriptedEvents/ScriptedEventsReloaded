using JetBrains.Annotations;
using PlayerRoles.FirstPersonControl;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class NoclipMethod : SynchronousMethod
{
    public override string Description => "Enables or disables access to noclip for specified players.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new BoolArgument("isAllowed")
    ];

    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var isAllowed = Args.GetBool("isAllowed");
        
        foreach (var player in players)
        {
            if (isAllowed)
            {
                FpcNoclip.PermitPlayer(player.ReferenceHub);
            }
            else
            {
                FpcNoclip.UnpermitPlayer(player.ReferenceHub);
            }
        }
    }
}