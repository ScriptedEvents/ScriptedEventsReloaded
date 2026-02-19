using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;
using UncomplicatedCustomRoles.Extensions;

namespace SER.Code.MethodSystem.Methods.UCRMethods;

// ReSharper disable once InconsistentNaming
[UsedImplicitly]
public class SetUCRRoleMethod : SynchronousMethod, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.Ucr;

    public override string Description => "Sets the UCR role of a player.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayerArgument("player"),
        new IntArgument("role id")
    ];

    public override void Execute()
    {
        var player = Args.GetPlayer("player");
        var roleId = Args.GetInt("role id");
        
        player.SetCustomRole(roleId);
    }
}