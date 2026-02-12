using System.Collections.ObjectModel;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using UnityEngine;

namespace SER.Code.MethodSystem.Methods.ObjectMethods;

public class RaycastMethod : ReferenceReturningMethod<GameObject>
{
    public override string Description { get; } = "Get gameobject infront of player";
    public override Argument[] ExpectedArguments { get; } = 
        [
            new PlayerArgument("player")
            {
                Description = "Single player which will return the game object the player is looking at",
            },
            new FloatArgument("max distance")
            {
                Description = "Maximum distance of the raycast",
            },
            new EnumArgument<MaskHelper.Layers>("layer")
            {
                Description = "Layer mask meaning what raycast can and can't go through.",
                DefaultValue = new (MaskHelper.Layers.TransparentFX | MaskHelper.Layers.Hitbox | MaskHelper.Layers.InvisibleCollider | MaskHelper.Layers.Skybox),
            }
        ];
    public override void Execute()
    {
        var player = Args.GetPlayer("player");
        var maxDistance = Args.GetFloat("max distance");
        var layer = Args.GetEnum<MaskHelper.Layers>("layer");
        var mask = MaskHelper.GetLayerMask(layer);
        
        if (!Physics.Raycast(player.Camera.position, player.Camera.forward, out var raycastHit,
                maxDistance, ~mask))
        {
            // Out of range
            ReturnValue = null;
            return;
        }

        ReturnValue = raycastHit.collider.gameObject;
    }
}

