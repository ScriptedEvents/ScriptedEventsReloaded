using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.TextMethods;

[UsedImplicitly]
public class SubTextMethod : ReturningMethod<TextValue>
{
    public override string Description => 
        "Removes certain amount of characters from beginning and end of a text value.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("text"),
        new IntArgument("beginning amount", 0)
        {
            Description = "The amount of characters to remove from the beginning of the text.",
            DefaultValue = new(0, null)
        },
        new IntArgument("end amount", 0)
        {
            Description = "The amount of characters to remove from the end of the text.",
        }
    ];

    public override void Execute()
    {
        var text = Args.GetText("text");
        var beginning = Args.GetInt("beginning amount");
        var end = Args.GetInt("end amount");

        try
        {
            ReturnValue = text[beginning..^end].ToDynamicTextValue(Script);
        }
        catch (ArgumentOutOfRangeException)
        {
            ReturnValue = new StaticTextValue(string.Empty);
        }
    }
}