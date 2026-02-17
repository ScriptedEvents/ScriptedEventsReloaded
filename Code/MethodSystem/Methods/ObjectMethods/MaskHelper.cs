namespace SER.Code.MethodSystem.Methods.ObjectMethods;

public class MaskHelper
{
    public static int GetLayerMask(Layers layers)
    {
        return (int)layers;
    }
    
    [Flags]
    public enum Layers
    {
        None = 0,

        Default                       = 1 << 0,
        TransparentFX                 = 1 << 1,
        IgnoreRaycast                 = 1 << 2,
        Water                         = 1 << 4,
        UI                            = 1 << 5,
        Player                        = 1 << 8,
        InteractableNoPlayerCollision = 1 << 9,
        Viewmodel                     = 1 << 10,
        RenderAfterFog                = 1 << 12,
        Hitbox                        = 1 << 13,
        Glass                         = 1 << 14,
        InvisibleCollider             = 1 << 16,
        Ragdoll                       = 1 << 17,
        CCTV                          = 1 << 18,
        Grenade                       = 1 << 20,
        Phantom                       = 1 << 21,
        OnlyWorldCollision            = 1 << 25,
        Door                          = 1 << 27,
        Skybox                        = 1 << 28,
        Fence                         = 1 << 29,
    }

}