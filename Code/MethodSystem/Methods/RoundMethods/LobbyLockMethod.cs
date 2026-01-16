using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;

namespace SER.Code.MethodSystem.Methods.RoundMethods;

[UsedImplicitly]
public class LobbyLockMethod : SynchronousMethod
{
    public override string Description => "Changes the lobby lock state.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new BoolArgument("new state")
    ];
    
    public override void Execute()
    {
        Round.IsLobbyLocked = Args.GetBool("new state");
    }
}