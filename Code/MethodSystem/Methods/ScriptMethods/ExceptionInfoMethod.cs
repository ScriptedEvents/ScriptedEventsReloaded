using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.ScriptMethods;

[UsedImplicitly]
public class ExceptionInfoMethod : ReturningMethod<TextValue>, IReferenceResolvingMethod
{
    public override string Description => IReferenceResolvingMethod.Desc.Get(this);
    
    public Type ResolvesReference => typeof(Exception);

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<Exception>("exception reference"),
        new OptionsArgument("info to get",
            "type",
            "message",
            "stackTrace")
    ];
    
    public override void Execute()
    {
        var exception = Args.GetReference<Exception>("exception reference");

        ReturnValue = new StaticTextValue(Args.GetOption("info to get") switch
        {
            "type" => exception.GetType().AccurateName,
            "message" => exception.Message,
            "stacktrace" => exception.StackTrace,
            _ => throw new TosoksFuckedUpException("out of order")
        });
    }
}