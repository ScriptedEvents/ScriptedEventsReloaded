using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.Helpers.Exceptions;
using SER.MethodSystem.BaseMethods;
using SER.MethodSystem.MethodDescriptors;
using SER.MethodSystem.Structures;
using SER.ValueSystem;

namespace SER.MethodSystem.Methods.ParsingMethods;

public class ParseResultMethod : ReturningMethod, ICanError, IReferenceResolvingMethod
{
    public override string Description => "Returns information from the parsing result.";

    public override Type[]? ReturnTypes => null;

    public Type ReferenceType => typeof(ParseResult);

    public string[] ErrorReasons =>
    [
        "Tried to access the value when the parsing was not successful"
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<ParseResult>("parsing result"),
        new OptionsArgument("info", 
            new("success", "Returns true if the parsing was successful"), 
            new("value", "The value that got parsed")
        )
    ];

    public override void Execute()
    {
        var parseResult = Args.GetReference<ParseResult>("parsing result");
        ReturnValue = Args.GetOption("info") switch
        {
            "success" => new BoolValue(parseResult.Success),
            "value" => parseResult.Value ?? throw new ScriptRuntimeError(ErrorReasons[0]),
            _ => throw new AndrzejFuckedUpException()
        };
    }
}