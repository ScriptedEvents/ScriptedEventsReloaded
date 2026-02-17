using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.AdminToyPropertyMethods;

[UsedImplicitly]
public class SetTextPropertiesMethod : SynchronousMethod
{
    public override string Description => $"Sets the properties of a {nameof(TextToy)}.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<TextToy>("text toy reference"),
        
        new TextArgument("label")           { DefaultValue = new(null, "not changing") },
        new FloatArgument("display width")  { DefaultValue = new(null, "not changing") },
        new FloatArgument("display height") { DefaultValue = new(null, "not changing") },
    ];
    public override void Execute()
    {
        var text = Args.GetReference<TextToy>("text toy reference");

        var width  = Args.GetNullableFloat("display width");
        var height = Args.GetNullableFloat("display height");
        
        if (Args.GetText("label") is { } label) text.TextFormat = label;

        text.DisplaySize = new(
            height ?? text.DisplaySize.x,
            width  ?? text.DisplaySize.y
            );
    }

}