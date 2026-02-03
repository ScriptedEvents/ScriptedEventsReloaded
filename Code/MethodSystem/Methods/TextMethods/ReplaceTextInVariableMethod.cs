using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.MethodSystem.Methods.TextMethods;

[UsedImplicitly]
public class ReplaceTextInVariableMethod : SynchronousMethod, ICanError
{
    public override string Description => "Replaces given values in a given text variable.";

    public string[] ErrorReasons =>
    [
        "The given variable is not a text variable."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new LiteralVariableArgument("text variable to perform the replacement on"),
        new TextArgument("text to replace"),
        new TextArgument("replacement text")
    ];
    
    public override void Execute()
    {
        var variable = Args.GetLiteralVariable("text variable to perform the replacement on");
        var text = Args.GetText("text to replace");
        var replacement = Args.GetText("replacement text");

        if (variable.Value is not TextValue textValue)
            throw new ScriptRuntimeError(this, ErrorReasons[0]);
        
        Script.AddVariable(
            new LiteralVariable(
                variable.Name,
                new DynamicTextValue(textValue.Value.Replace(text, replacement), Script)
            )
        );
    }
}