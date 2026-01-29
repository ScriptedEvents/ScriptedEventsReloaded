using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.TextMethods;

[UsedImplicitly]
public class PadTextMethod : ReturningMethod<TextValue>, ICanError, IAdditionalDescription
{
    public override string Description => "Fills the text from the left or right with the given character " +
                                          "until the specified length is met";

    public string AdditionalDescription => "The \"character\" argument must have EXACTLY 1 character in it.";
    
    public string[] ErrorReasons =>
    [
        "The \"character\" argument doesn't have EXACTLY 1 character in it."
    ];
    
    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("text to pad"),
        new OptionsArgument("pad direction",
            "left",
            "right"),
        new IntArgument("length"),
        new TextArgument("character")
    ];
    
    public override void Execute()
    {
        var text = Args.GetText("text to pad");
        var direction = Args.GetOption("pad direction");
        var length = Args.GetInt("length");
        var character = Args.GetText("character");
        
        if (character.Length != 1)
            throw new ScriptRuntimeError(this, ErrorReasons[0]);

        ReturnValue = (direction switch
        {
            "left" => text.PadLeft(length, character[0]),
            "right" => text.PadRight(length, character[0]),
            _ => throw new TosoksFuckedUpException("out of order")
        }).ToDynamicTextValue(Script);
    }
}