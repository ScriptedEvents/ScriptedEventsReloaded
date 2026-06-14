using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.TextMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Text_ReplaceMethod : ReturningMethod<TextValue>
{
    public override string Description => "Replaces given values in a given text.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("value to perform the replacement on"),
        new TextArgument("text to replace"),
        new TextArgument("replacement text")
    ];
    
    public override void Execute()
    {
        var value = Args.GetText("value to perform the replacement on");
        var text = Args.GetText("text to replace");
        var replacement = Args.GetText("replacement text");
        
        ReturnValue = new DynamicTextValue(value.Replace(text, replacement), Script);
    }
}