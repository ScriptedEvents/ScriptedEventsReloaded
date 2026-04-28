using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.CustomRoleMethods;

[UsedImplicitly]
public class SetCRoleMethod : SynchronousMethod
{
    public override string Description => "Assigns a custom role to a player.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new CustomRoleArgument("custom role")
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var customRole = Args.GetCustomRole("custom role");
        
        foreach (var player in players)
            customRole.AssignPlayer(player);
    }
}