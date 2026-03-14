using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.TextMethods;

[UsedImplicitly]
public class ContainsTextMethod : ReturningMethod
{
    public override string Description => "Returns true or false indicating if the provided text contains a provided value.";

    public override Argument[] ExpectedArguments => 
    [
        new TextArgument("text"),
        new TextArgument("text to check for"),
    ];
    public override void Execute()
    {
        var stringToCheck = Args.GetText("text");
        var substringToCheck = Args.GetText("text to check for");
        ReturnValue = new BoolValue(stringToCheck.Contains(substringToCheck));
    }

    public override TypeOfValue Returns => new SingleTypeOfValue(typeof(BoolValue));
}