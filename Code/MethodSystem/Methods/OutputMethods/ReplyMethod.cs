using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.OutputMethods;

[UsedImplicitly]
public class ReplyMethod : SynchronousMethod
{
    public override string Description => 
        "Sends a message to the place where the script was run from. Usually used for replying to the command sender.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("message")
    ];
    
    public override void Execute()
    {
        Script.Executor.Reply(Args.GetText("message"), Script);
    }
}