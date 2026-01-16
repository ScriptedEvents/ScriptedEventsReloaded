using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using PlayerRoles.PlayableScps.Scp079;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;

namespace SER.Code.MethodSystem.Methods.Scp079Methods;

[UsedImplicitly]
public class Set079ExpMethod : SynchronousMethod
{
    public override string Description => "Sets the EXP of the given player(s) if they are SCP-079";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new IntArgument("exp", 0)
    ];

    public override void Execute()
    {
        var plrs = Args.GetPlayers("players");
        var value = Args.GetInt("exp");
        
        foreach (Player p in plrs)
        {
            if (p.RoleBase is not Scp079Role scp)
            {
                continue;
            }
            
            if (scp.SubroutineModule.TryGetSubroutine(out Scp079TierManager tier))
            {
                tier.TotalExp = value;
            }
        }
    }
}
