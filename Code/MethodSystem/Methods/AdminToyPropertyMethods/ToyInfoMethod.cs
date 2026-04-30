using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.AdminToyPropertyMethods;

[UsedImplicitly]
public class ToyInfoMethod : ReturningMethod<NumberValue>
{
    public override string Description => "Returns information about an Admin Toy";

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<AdminToy>("toy reference"),
        new OptionsArgument("property",
            "netId",
            "posX",
            "posY",
            "posZ",
            "rotX",
            "rotY",
            "rotZ",
            "scaleX",
            "scaleY",
            "scaleZ")
    ]; // TODO: add every property from every toy type

    public override void Execute()
    {
        var toy = Args.GetReference<AdminToy>("toy reference");

        ReturnValue = Args.GetOption("property") switch
        {
            "netid" => toy.Base.netId,
            "posx" => (decimal)toy.Position.x,
            "posy" => (decimal)toy.Position.y,
            "posz" => (decimal)toy.Position.z,
            "rotx" => (decimal)toy.Rotation.eulerAngles.x,
            "roty" => (decimal)toy.Rotation.eulerAngles.y,
            "rotz" => (decimal)toy.Rotation.eulerAngles.z,
            "scalex" => (decimal)toy.Scale.x,
            "scaley" => (decimal)toy.Scale.y,
            "scalez" => (decimal)toy.Scale.z,
            _ => throw new TosoksFuckedUpException("out of order")
        };
    }
}