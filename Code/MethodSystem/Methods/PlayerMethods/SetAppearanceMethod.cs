using Exiled.API.Extensions;
using Exiled.API.Features;
using JetBrains.Annotations;
using PlayerRoles;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class SetAppearanceMethod : SynchronousMethod, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.Exiled;
    
    public override string Description => "Changes the appearance of a player (or reskins)";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players whose appearance will be changed"),
        new EnumArgument<RoleTypeId>("role to change appearance to"),
        new PlayersArgument("players who will see the change")
        {
            DefaultValue = new(null, "everyone")
        }
    ];

    public override void Execute()
    {
        var labApiPlayers = Args.GetPlayers("players whose appearance will be changed");
        Player[] players = [];
        for (uint i = 0; i < labApiPlayers.Length; i++)
            players[i] = Player.Get(labApiPlayers[i]);
        
        var role = Args.GetEnum<RoleTypeId>("role to change appearance to");
        var potentialTargets = Args.GetPlayers("players who will see the change").MaybeNull();

        if (potentialTargets != null)
        {
            Player[] targets = [];
            for (uint i = 0; i < potentialTargets.Length; i++)
                targets[i] = Player.Get(potentialTargets[i]);
            
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