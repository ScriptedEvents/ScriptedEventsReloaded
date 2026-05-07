using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Methods.CustomRoleMethods.Structures;

namespace SER.Code.MethodSystem.Methods.CustomRoleMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class CRole_SetMethod : SynchronousMethod
{
    public override string Description => "Assigns a custom role to a player.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new CustomRoleArgument("custom role")
        {
            DefaultValue = new(null, "Removes the currect custom role from the player", true)
        }
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        if (Args.GetCustomRole("custom role") is  { } role)
        {
            foreach (var player in players) role.AssignPlayer(player);
        }
        else
        {
            foreach (var player in players) CRole.RemoveRoleFrom(player);
        }
    }
}