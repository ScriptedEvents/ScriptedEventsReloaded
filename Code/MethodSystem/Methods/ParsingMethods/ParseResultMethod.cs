using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.MethodSystem.Structures;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.ParsingMethods;

[UsedImplicitly]
public class ParseResultMethod : ReturningMethod, ICanError, IReferenceResolvingMethod
{
    public override string Description => "Returns information from the parsing result.";

    public override TypeOfValue Returns => new UnknownTypeOfValue();

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
            "value" => parseResult.Value ?? throw new ScriptRuntimeError(this, ErrorReasons[0]),
            _ => throw new AndrzejFuckedUpException()
        };
    }
}