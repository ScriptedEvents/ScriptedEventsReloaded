using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.FlagSystem.Flags;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.CommandMethods;

[UsedImplicitly]
public class ResetPlayerCommandCooldownMethod : SynchronousMethod
{
    public override string Description => "Resets a player's command cooldown from the 'CustomCommand' flag.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<CustomCommandFlag.CustomCommand>("command"),
        new PlayerArgument("player")
    ];
    
    public override void Execute()
    {
        Args
            .GetReference<CustomCommandFlag.CustomCommand>("command")
            .NextEligibleDateForPlayer
            .Remove(Args.GetPlayer("player"));
    }
}