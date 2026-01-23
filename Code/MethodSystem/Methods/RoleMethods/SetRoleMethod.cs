using JetBrains.Annotations;
using PlayerRoles;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.RoleMethods;

[UsedImplicitly]
public class SetRoleMethod : SynchronousMethod
{
    public override string Description => "Sets a role for players.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new EnumArgument<RoleTypeId>("newRole"),
        new EnumArgument<RoleSpawnFlags>("spawnFlags")
        {
            DefaultValue = new(RoleSpawnFlags.All, null)
        },
        new EnumArgument<RoleChangeReason>("reason")
        {
            DefaultValue = new(RoleChangeReason.RemoteAdmin, null)
        }
    ];

    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var newRole = Args.GetEnum<RoleTypeId>("newRole");
        var reason = Args.GetEnum<RoleChangeReason>("reason");
        var flags = Args.GetEnum<RoleSpawnFlags>("spawnFlags");
        
        foreach (var player in players) 
            player.SetRole(newRole, reason, flags);
    }
}