using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;
using SER.MethodSystem.Structures;
using SER.TokenSystem.Tokens;
using SER.ValueSystem;

namespace SER.MethodSystem.Methods.NumberMethods;

public class TryParseNumberMethod : ReferenceReturningMethod<ParseResult<NumberValue>>
{
    public override string Description => "Tries to parse a given value to a number.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new AnyValueArgument("value to parse")
    ];

    public override void Execute()
    {
        var valueToParse = Args.GetAnyValue("value to parse");
        if (valueToParse is NumberValue numVal)
        {
            ReturnValue = new(numVal.Value);
            return;
        }

        if (BaseToken.TryParse<NumberToken>(valueToParse.ToString(), Script).WasSuccessful(out var token))
        {
            ReturnValue = new(token.Value);
            return;
        }
        
        ReturnValue = new(null);
    }
}