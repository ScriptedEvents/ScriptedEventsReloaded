using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.RoundMethods;

[UsedImplicitly]
public class FriendlyFireMethod : SynchronousMethod
{
    public override string Description => "Changes friendly fire mode.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new BoolArgument("enabled?")
    ];
    
    public override void Execute()
    {
        ServerConsole.FriendlyFire = Args.GetBool("enabled?");
        ServerConfigSynchronizer.Singleton.RefreshMainBools();
    }
}