using System.Globalization;
using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.NumberMethods;

[UsedImplicitly]
public class HexToIntMethod : ReturningMethod<NumberValue>, ICanError, IAdditionalDescription
{
    public override string Description => "Parses a hexadecimal number back to a number value";

    public string AdditionalDescription => "The hex number can start (but doesn't have to) with 0x or #";

    public string[] ErrorReasons =>
    [
        "The provided string does not represent a hexadecimal number."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("hex number")
    ];

    public override void Execute()
    {
        var hex = Args.GetText("hex number");
        
        if (hex.StartsWith("0x"))
            hex = hex[2..];
        
        if (hex.StartsWith("#"))
            hex = hex[1..];
        
        ReturnValue = int.TryParse(
            hex, 
            NumberStyles.HexNumber, 
            NumberFormatInfo.InvariantInfo, 
            out var result) 
            ? result 
            : throw new ScriptRuntimeError(this, ErrorReasons[0]);
    }
}