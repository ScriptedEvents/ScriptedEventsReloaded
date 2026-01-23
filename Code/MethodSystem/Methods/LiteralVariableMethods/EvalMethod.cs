using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.LiteralVariableMethods;

[UsedImplicitly]
public class EvalMethod : ReturningMethod
{
    public override string Description => 
        "Evaluates the provided expression and returns the result. Used for math operations.";
    
    public override TypeOfValue Returns => new UnknownTypeOfValue();

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("value")
    ];

    public override void Execute()
    {
        var value = Args.GetText("value");
        if (NumericExpressionReslover.CompileExpression(value, Script).HasErrored(out var error, out var expression))
        {
            throw new ScriptRuntimeError(this, error);
        }

        if (expression.Evaluate().HasErrored(out var error2, out var result))
        {
            throw new ScriptRuntimeError(this, error2);
        }
        
        ReturnValue = Value.Parse(result);
    }
}