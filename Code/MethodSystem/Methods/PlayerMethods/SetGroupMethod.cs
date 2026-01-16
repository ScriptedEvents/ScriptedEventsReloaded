using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class SetGroupMethod : SynchronousMethod
{
    public override string Description => "Sets or removes group from specified players";
    
    public override Argument[] ExpectedArguments { get; } = 
    [
        new PlayersArgument("players"),
        new TextArgument("group")
        {
            Description = "Name of the group or set to NONE if you want to remove group from specified players"
        }
    ];

    public override void Execute()
    {
        string group = Args.GetText("group");
        var players = Args.GetPlayers("players");
        
        foreach (Player plr in players)
        {
            plr.UserGroup = group == "NONE" 
                ? null 
                : ServerStatic.PermissionsHandler.GetGroup(group);
        }
    }
}