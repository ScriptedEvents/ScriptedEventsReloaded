using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.EventMethods;

[UsedImplicitly]
public class IsAllowedMethod : SynchronousMethod, IAdditionalDescription
{
    public override string Description => "Allows or cancels the event that started this script.";

    public string AdditionalDescription =>
        "This only works in a cancellable event, before any Wait or other pause. " +
        "It cannot cancel the current event while SafeScripts is enabled because the safety pause lets the event continue first.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new BoolArgument("isAllowed")
    ];

    public override void Execute()
    {
        Script.SetEventAllowed(Args.GetBool("isAllowed"));
    }
}
