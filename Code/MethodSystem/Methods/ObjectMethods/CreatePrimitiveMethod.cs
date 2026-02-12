using AdminToys;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using UnityEngine;
using PrimitiveObjectToy = LabApi.Features.Wrappers.PrimitiveObjectToy;

namespace SER.Code.MethodSystem.Methods.ObjectMethods;

public class CreatePrimitiveMethod : ReferenceReturningMethod<GameObject>
{
    public override string Description { get; } = "Method to create primitive";

    public override Argument[] ExpectedArguments { get; } =
    [
        new EnumArgument<PrimitiveType>("primitiveType"),
        new ColorArgument("color"),

        new FloatArgument("x_position"),
        new FloatArgument("y_position"),
        new FloatArgument("z_position"),

        new FloatArgument("x_rotation")
        {
            DefaultValue = new(1f),
        },
        new FloatArgument("y_rotation")
        {
            DefaultValue = new(1f),
        },
        new FloatArgument("z_rotation")
        {
            DefaultValue = new(1f),
        },

        new FloatArgument("x_scale")
        {
            DefaultValue = new(1f)
        },
        new FloatArgument("y_scale")
        {
            DefaultValue = new(1f)
        },
        new FloatArgument("z_scale")
        {
            DefaultValue = new(1f)
        },

        new BoolArgument("IsNetworked")
        {
            DefaultValue = new(true),
        },

        new EnumArgument<PrimitiveFlags>("flags")
        {
            DefaultValue = new(PrimitiveFlags.Collidable | PrimitiveFlags.Visible),
        }
    ];

    public override void Execute()
    {
        var type = Args.GetEnum<PrimitiveType>("primitiveType");
        var color = Args.GetColor("color");
        var flags = Args.GetEnum<PrimitiveFlags>("flags");

        var xPos = Args.GetFloat("x_position");
        var yPos = Args.GetFloat("y_position");
        var zPos = Args.GetFloat("z_position");

        var xRot = Args.GetFloat("x_rotation");
        var yRot = Args.GetFloat("y_rotation");
        var zRot = Args.GetFloat("z_rotation");

        var xScl = Args.GetFloat("x_scale");
        var yScl = Args.GetFloat("y_scale");
        var zScl = Args.GetFloat("z_scale");

        var spawn = Args.GetBool("IsNetworked");

        var pos = new Vector3(xPos, yPos, zPos);
        var rot = new Vector3(xRot, yRot, zRot);
        var scl = new Vector3(xScl, yScl, zScl);

        var prim = PrimitiveObjectToy.Create(pos, Quaternion.Euler(rot), scl, null, false);
        prim.Base.NetworkPrimitiveType = type;
        prim.Color = color;
        prim.Flags = flags;

        if (spawn)
            prim.Spawn();
    }
}