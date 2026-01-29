using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.TextMethods;

[UsedImplicitly]
public class TextLengthMethod : ReturningMethod<NumberValue>
{
    public override string Description => "Returns the length of the text.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("text")
    ];
    
    public override void Execute()
    {
        ReturnValue = Args.GetText("text").Length;
    }
}