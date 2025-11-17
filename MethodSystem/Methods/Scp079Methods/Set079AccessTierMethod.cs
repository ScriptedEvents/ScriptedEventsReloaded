using LabApi.Features.Wrappers;
using PlayerRoles.PlayableScps.Scp079;
using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;

namespace SER.MethodSystem.Methods.Scp079Methods;

public class Set079AccessTierMethod : SynchronousMethod
{
    public override string Description => "Sets the Access Tier of the given player(s) if they are SCP-079";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new IntArgument("tier", 1, 5)
    ];

    public override void Execute()
    {
        var plrs = Args.GetPlayers("players");
        var value = Args.GetInt("tier");
        
        foreach (Player p in plrs)
        {
            if (p.RoleBase is not Scp079Role scp)
            {
                continue;
            }
            
            var levelIndex = value - 1;
            if (scp.SubroutineModule.TryGetSubroutine(out Scp079TierManager tier))
            {
                tier.TotalExp = levelIndex != 0 ? tier.AbsoluteThresholds[levelIndex - 1] : 0;
            }
        }
    }
}
