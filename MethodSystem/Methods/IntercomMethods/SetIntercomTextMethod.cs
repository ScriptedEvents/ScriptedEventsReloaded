using SER.MethodSystem.BaseMethods;
using SER.ArgumentSystem.BaseArguments;
using SER.ArgumentSystem.Arguments;
using PlayerRoles.Voice;
using SER.MethodSystem.MethodDescriptors;

namespace SER.MethodSystem.Methods.IntercomMethods;

public class SetIntercomTextMethod : SynchronousMethod, IAdditionalDescription
{
    public override string? Description => "Sets the text on the Intercom.";

    public string AdditionalDescription => "Resets the intercom text if given text is empty.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("text")
    ];

    public override void Execute()
    {
        IntercomDisplay.TrySetDisplay(Args.GetText("text"));
    }
}
