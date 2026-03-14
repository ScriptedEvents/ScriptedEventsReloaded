using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using PlayerRoles;
using PlayerRoles.Ragdolls;
using PlayerStatsSystem;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;
using UnityEngine;

namespace SER.Code.MethodSystem.Methods.MapMethods;

[UsedImplicitly]
public class SpawnRagdollMethod : SynchronousMethod, ICanError
{
    public override string Description => "Spawns a ragdoll.";

    public override Argument[] ExpectedArguments =>
    [
        new EnumArgument<RoleTypeId>("role"),
        new TextArgument("name"),
        new FloatArgument("x position"),
        new FloatArgument("y position"),
        new FloatArgument("z position"),
        new FloatArgument("x size")
            { DefaultValue = new(null, "default role x scale") },
        new FloatArgument("y size")
            { DefaultValue = new(null, "default role y scale") },
        new FloatArgument("z size")
            { DefaultValue = new(null, "default role z scale") },
        new FloatArgument("x rotation")
            { DefaultValue = new(0f, null) },
        new FloatArgument("y rotation")
            { DefaultValue = new(0f, null) },
        new FloatArgument("z rotation")
            { DefaultValue = new(0f, null) },
        new FloatArgument("w rotation")
            { DefaultValue = new(1f, null) },
        new AnyValueArgument("damage handler")
        {
            DefaultValue = new(new CustomReasonDamageHandler(""), "Damage reason will be blank"),
            Description = $"Accepts a {nameof(TextValue)} or a {nameof(DamageHandlerBase)} reference."
        },
    ];
    
    public override void Execute()
    {
        var role = Args.GetEnum<RoleTypeId>("role");
        var name = Args.GetText("name");
        
        var xPosition = Args.GetFloat("x position");
        var yPosition = Args.GetFloat("y position");
        var zPosition = Args.GetFloat("z position");
        
        var defaultSize = RagdollManager.GetDefaultScale(role);
        
        var xSize = Args.GetNullableFloat("x size") ?? defaultSize.x;
        var ySize = Args.GetNullableFloat("y size") ?? defaultSize.y;
        var zSize = Args.GetNullableFloat("z size") ?? defaultSize.z;
        
        var xRotation = Args.GetFloat("x rotation");
        var yRotation = Args.GetFloat("y rotation");
        var zRotation = Args.GetFloat("z rotation");
        var wRotation = Args.GetFloat("w rotation");
        
        var value = Args.GetAnyValue("damage handler");
        
        var position = new Vector3(xPosition, yPosition, zPosition);
        var rotation = new Quaternion(xRotation, yRotation, zRotation, wRotation);
        var size = new Vector3(xSize, ySize, zSize);
        
        DamageHandlerBase? damageHandler = null;
        
        switch (value)
        {
            case ReferenceValue referenceValue:
            {
                if (referenceValue.Value is DamageHandlerBase handler)
                    damageHandler = handler;
                else
                    throw new ScriptRuntimeError(this, ErrorReasons[1]);
                break;
            }
            case TextValue textValue:
                damageHandler = new CustomReasonDamageHandler(textValue.StringRep);
                break;
        }
        
        if (damageHandler is null)
            throw new ScriptRuntimeError(this, ErrorReasons[0]);
        
        Ragdoll.SpawnRagdoll(role, position, rotation, damageHandler, name, size);
    }

    public string[] ErrorReasons =>
    [
        $"Damage handler value must be a {nameof(DamageHandlerBase)} reference or a {nameof(TextValue)}.",
        $"The provided reference value was not a {nameof(DamageHandlerBase)} reference."
    ];
}