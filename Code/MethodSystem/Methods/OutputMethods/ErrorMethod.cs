using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;

namespace SER.Code.MethodSystem.Methods.OutputMethods;

[UsedImplicitly]
public class ErrorMethod : SynchronousMethod
{
    public override string Description => "Sends an error message.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("error")
    ];
    
    public override void Execute()
    {
        Script.Executor.Error(Args.GetText("error"), Script);
    }
}