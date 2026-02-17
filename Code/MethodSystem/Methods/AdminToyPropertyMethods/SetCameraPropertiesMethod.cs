using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;

namespace SER.Code.MethodSystem.Methods.AdminToyPropertyMethods;

[UsedImplicitly]
public class SetCameraPropertiesMethod : SynchronousMethod, ICanError
{
    public override string Description => $"Sets the properties of a {nameof(CameraToy)}.";

    public string[] ErrorReasons =>
    [
        "up constraint has to be lower or equal to down constraint",
        "left constraint has to be lower or equal to right constraint",
        "minimal zoom has to be lower or equal to maximal zoom",
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<CameraToy>("camera reference"),
        
        new TextArgument("label")
            { DefaultValue = new(null, "not changing") },
        new FloatArgument("up constraint", -180, 180)
            { DefaultValue = new(null, "not changing") },
        new FloatArgument("down constraint", -180, 180)
            { DefaultValue = new(null, "not changing") },
        new FloatArgument("left constraint", -180, 180)
            { DefaultValue = new(null, "not changing") },
        new FloatArgument("right constraint", -180, 180)
            { DefaultValue = new(null, "not changing") },
        new FloatArgument("minimal zoom", 0, 1)
            { DefaultValue = new(null, "not changing") },
        new FloatArgument("maximal zoom", 0, 1)
            { DefaultValue = new(null, "not changing") },
    ];
    public override void Execute()
    {
        var camera = Args.GetReference<CameraToy>("camera reference");

        var up      = Args.GetNullableFloat("up constraint");
        var down    = Args.GetNullableFloat("down constraint");
        var left    = Args.GetNullableFloat("left constraint");
        var right   = Args.GetNullableFloat("right constraint");
        var minZoom = Args.GetNullableFloat("minimal zoom");
        var maxZoom = Args.GetNullableFloat("maximal zoom");

        if (up > down)         throw new ScriptRuntimeError(this, ErrorReasons[0]);
        if (left > right)      throw new ScriptRuntimeError(this, ErrorReasons[1]);
        if (minZoom > maxZoom) throw new ScriptRuntimeError(this, ErrorReasons[2]);
        
        if (Args.GetText("label") is { } label) camera.Label = label;

        camera.VerticalConstraints = new(
            up ?? camera.VerticalConstraints.x,
            down ?? camera.VerticalConstraints.y
            );
        camera.HorizontalConstraint = new(
            left ?? camera.HorizontalConstraint.x,
            right ?? camera.HorizontalConstraint.y
            );
        camera.ZoomConstraints = new(
            minZoom ?? camera.ZoomConstraints.x,
            maxZoom ?? camera.ZoomConstraints.y
            );
    }

}