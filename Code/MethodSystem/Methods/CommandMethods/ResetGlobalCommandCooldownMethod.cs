using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.FlagSystem.Flags;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.CommandMethods;

[UsedImplicitly]
public class ResetGlobalCommandCooldownMethod : SynchronousMethod
{
    public override string Description => "Resets the global command cooldown for the 'CustomCommand' flag.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<CustomCommandFlag.CustomCommand>("command")
    ];
    
    public override void Execute()
    {
        Args.GetReference<CustomCommandFlag.CustomCommand>("command").NextEligibleDateForGlobal = null;
    }
}