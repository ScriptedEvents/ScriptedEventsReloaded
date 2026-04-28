using Mirror;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.DummyMethods;

[UsedImplicitly]
public class DestroyDummyMethod : SynchronousMethod
{
    public override string Description => "Destroys a dummy off of the server.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("dummies to destroy")
    ];
    
    public override void Execute()
    {
        foreach (var player in Args.GetPlayers("dummies to destroy"))
        {
            if (!player.IsDummy) continue;
            NetworkServer.Destroy(player.GameObject);
        }
    }
}