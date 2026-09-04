using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers;
using SER.Code.Integrations.Ucr;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.UCRMethods;

// ReSharper disable once InconsistentNaming
[UsedImplicitly]
public class UCR_SetRoleMethod : SynchronousMethod, ICanError, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.UncomplicatedCustomRoles;

    public override string Description => "Sets the UCR role of a player.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayerArgument("player"),
        new IntArgument("role id")
    ];

    public override void Execute()
    {
        UcrBridge.SetRole(Args.GetPlayer("player"), Args.GetInt("role id"));
    }

    public string[] ErrorReasons => ["The role ID is not registered in UCR."];
}
