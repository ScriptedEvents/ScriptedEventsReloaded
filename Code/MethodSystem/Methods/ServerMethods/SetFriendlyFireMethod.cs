using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.ServerMethods;

[UsedImplicitly]
public class SetFriendlyFireMethod : SynchronousMethod
{
    public override string Description => "Changes friendly fire mode.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new BoolArgument("enabled?")
    ];
    
    public override void Execute()
    {
        Server.FriendlyFire = Args.GetBool("enabled?");
    }
}