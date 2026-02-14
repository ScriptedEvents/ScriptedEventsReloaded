using Exiled.API.Extensions;
using Exiled.API.Features;
using JetBrains.Annotations;
using PlayerRoles;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class SetAppearanceMethod : SynchronousMethod, IDependOnFramework
{
    public IDependOnFramework.Type DependsOn => IDependOnFramework.Type.Exiled;
    
    public override string Description => "Changes the appearance of a player (or reskins)";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players whose appearance will be changed"),
        new EnumArgument<RoleTypeId>("role to change appearance to"),
        new PlayerArgument("players who will see the change")
        {
            DefaultValue = new(null, "everyone")
        }
    ];

    public override void Execute()
    {
        var players = Args
            .GetPlayers("players whose appearance will be changed")
            .Select(Player.Get)
            .ToArray();
        var role = Args.GetEnum<RoleTypeId>("role to change appearance to");
        var potentialTargets = Args.GetPlayers("players who will see the change").MaybeNull();

        if (potentialTargets != null)
        {
            var targets = potentialTargets.Select(Player.Get).ToArray();
            foreach (var player in players)
            {
                player.ChangeAppearance(role, targets);
            }
        }
        else
        {
            foreach (var player in players)
            {
                player.ChangeAppearance(role);
            }
        }
    }
}