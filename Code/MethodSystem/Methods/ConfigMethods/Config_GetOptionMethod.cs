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
        "It's advised that you set the 'default value' argument if you don't want an error to occur if no key is found.";

    public string[] ErrorReasons { get; } =
    [
        "The requested key was not found in the config."
    ];

    public override string Description => "Tries to get a value from a config.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<CustomConfig>("config"),
        new TextArgument("key"),
        new AnyValueArgument("default value")
        {
            Description = "The value to return if the key is not found.",
            DefaultValue = new(null, "no default value")
        }
    ];

    public override void Execute()
    {
        var config = Args.GetReference<CustomConfig>("config");
        var key = Args.GetText("key");
        var defaultValue = Args.GetAnyValue("default value");

        if (config.GetValue(key) is { } value)
        {
            ReturnValue = Value.Parse(value);
            return;
        }

        if (defaultValue is not null)
        {
            ReturnValue = defaultValue;
            return;
        }

        throw new ScriptRuntimeError(this, $"Key '{key}' was not found in the config.");
    }
}
