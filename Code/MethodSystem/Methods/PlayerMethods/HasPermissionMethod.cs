using LabApi.Features.Permissions;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class HasPermissionMethod : ReturningMethod<BoolValue>
{
    public override string Description => "Checks if a player has a specific permission.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayerArgument("player"),
        new TextArgument("permissions")
        {
            ConsumesRemainingValues = true,
            Description = "'true' is returned when player has any of the provided permissions"
        }
    ];
    
    public override void Execute()
    {
        ReturnValue = Args
            .GetPlayer("player")
            .HasAnyPermission(Args.GetRemainingArguments<string, TextArgument>("permissions"));
    }
}