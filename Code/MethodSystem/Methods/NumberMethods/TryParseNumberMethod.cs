using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.MethodSystem.Structures;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.NumberMethods;

[UsedImplicitly]
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