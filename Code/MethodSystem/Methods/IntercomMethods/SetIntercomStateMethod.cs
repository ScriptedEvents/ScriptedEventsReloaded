using JetBrains.Annotations;
using PlayerRoles.Voice;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;

namespace SER.Code.MethodSystem.Methods.IntercomMethods;

[UsedImplicitly]
public class SetIntercomStateMethod : SynchronousMethod
{
    public override string Description => "Sets the state of usage of the intercom.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new EnumArgument<IntercomState>("state")
    ];

    public override void Execute()
    {
        Intercom.State = Args.GetEnum<IntercomState>("state");
    }
}