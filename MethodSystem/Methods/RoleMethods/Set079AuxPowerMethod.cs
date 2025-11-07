using LabApi.Features.Wrappers;
using PlayerRoles.PlayableScps.Scp079;
using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;
using System.Collections.Generic;

namespace SER.MethodSystem.Methods.RoleMethods;
public class Set079AuxPowerMethod : SynchronousMethod
{
    public override string Description => "Sets players EXP if he is SCP-079";

    public override Argument[] ExpectedArguments =>
    [
        new PlayersArgument("players"),
        new IntArgument("exp")
    ];

    public override void Execute()
    {
        List<Player> pls = Args.GetPlayers("players");
        int exp = Args.GetInt("exp");
        foreach(Player p in pls)
        {
            if(p.RoleBase is Scp079Role scp)
            {
                if(scp.SubroutineModule.TryGetSubroutine<Scp079TierManager>(out Scp079TierManager tier))
                {
                    tier.TotalExp = exp;
                }
            }
        }
    }
}
