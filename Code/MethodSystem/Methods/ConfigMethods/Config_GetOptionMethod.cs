using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Methods.ConfigMethods.Structures;
using SER.Code.MethodSystem.Structures;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;

namespace SER.Code.MethodSystem.Methods.ConfigMethods;

// ReSharper disable once InconsistentNaming
[UsedImplicitly]
public class Config_GetOptionMethod : ReturningMethod, ICanError, IAdditionalDescription
{
    public override TypeOfValue Returns => new UnknownTypeOfValue();

    public string AdditionalDescription =>
        "It's advised that you use 'attempt' statement when trying to get an option from the config, as many things " +
        "can go wrong during the process.";

    public string[] ErrorReasons { get; } =
    [
        "No key was found in the config."
    ];

    public override string Description => "Tries to get a value from a config.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<CustomConfig>("config"),
        new TextArgument("keys")
        {
            ConsumesRemainingValues = true,
            Description = "If the config is nested, you can provide multiple keys to traverse the option tree."
        }
    ];

    public override void Execute()
    {
        var config = Args.GetReference<CustomConfig>("config");
        var keys = Args.GetRemainingArguments<string, TextArgument>("keys");

        ReturnValue = Value.Parse(
            config.GetValue(keys) ?? throw new ScriptRuntimeError(this, ErrorReasons[0])
        );
    }
}