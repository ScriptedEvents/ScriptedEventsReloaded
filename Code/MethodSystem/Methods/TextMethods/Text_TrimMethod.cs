using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.TextMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Text_TrimMethod : ReturningMethod<TextValue>
{
    public override string Description => "Trims the text value from whitspaces at the beginning and end.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("text")
    ];
    
    public override void Execute()
    {
        ReturnValue = Args
            .GetText("text")
            .Trim()
            .ToDynamicTextValue(Script);
    }
}