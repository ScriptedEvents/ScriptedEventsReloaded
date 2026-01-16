using JetBrains.Annotations;
using PlayerRoles.Voice;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.MethodSystem.MethodDescriptors;

namespace SER.Code.MethodSystem.Methods.IntercomMethods;

[UsedImplicitly]
public class SetIntercomTextMethod : SynchronousMethod, IAdditionalDescription
{
    public override string Description => "Sets the text on the Intercom.";

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
