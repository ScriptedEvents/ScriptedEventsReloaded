using JetBrains.Annotations;
using LabApi.Features.Console;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.OutputMethods;

[UsedImplicitly]
public class PrintMethod : SynchronousMethod
{
    public override string Description => "Prints the text provided to the server console.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("text"),
        new EnumArgument<ConsoleColor>("color")
        {
            DefaultValue = new(ConsoleColor.Green, null)
        }
    ];

    public override void Execute()
    {
        Logger.Raw(Args.GetText("text"), Args.GetEnum<ConsoleColor>("color"));
    }
}