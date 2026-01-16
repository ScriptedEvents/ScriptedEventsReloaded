using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.TextMethods;

[UsedImplicitly]
public class JoinTextMethod : ReturningMethod<TextValue>
{
    public override string Description => "Joins provided text values.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("text to join")
        {
            ConsumesRemainingValues = true
        }
    ];
    
    public override void Execute()
    {
        var texts = Args.GetRemainingArguments<string, TextArgument>("text to join");
        ReturnValue = new TextValue(string.Join("", texts));
    }
}