using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.MethodSystem.Methods.DictionaryMethods.Structures;
using SER.Code.ValueSystem.Other;

namespace SER.Code.MethodSystem.Methods.DictionaryMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Dict_GetMethod : ReturningMethod, ICanError
{
    public override string Description => "Gets a value associated with a key from a dictionary.";
    
    public override TypeOfValue Returns => new UnknownTypeOfValue();

    public string[] ErrorReasons =>
    [
        "There is no key with the given name in the dictionary."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<Dict>("dictionary"),
        new AnyValueArgument("key")
    ];
    
    public override void Execute()
    {
        var dictionary = Args.GetReference<Dict>("dictionary");
        var key = Args.GetAnyValue("key");

        if (dictionary.TryGetValue(key, out var value))
        {
            ReturnValue = value;
        }
        else
        {
            throw new ScriptRuntimeError(this, $"Provided key '{key}' is not in the dictionary.");
        }
    }
}