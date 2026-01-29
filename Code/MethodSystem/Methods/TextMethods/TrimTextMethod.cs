using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.TextMethods;

[UsedImplicitly]
public class TrimTextMethod : ReturningMethod<TextValue>
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