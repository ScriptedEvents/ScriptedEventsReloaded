using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Methods.CustomRoleMethods.Structures;

namespace SER.Code.MethodSystem.Methods.CustomRoleMethods;

[UsedImplicitly]
public class RemoveCRoleMethod : SynchronousMethod
{
    public override string Description => "Removes current custom role (if any) from specified players.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players")
    ];
    
    public override void Execute()
    {
        foreach (var player in Args.GetPlayers("players")) 
            CRole.RemoveRoleFrom(player);
    }
}