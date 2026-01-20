using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using Mirror;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.MethodSystem.MethodDescriptors;
using UnityEngine;

namespace SER.Code.MethodSystem.Methods.PickupMethods;

[UsedImplicitly]
public class SpawnPickupPosMethod : SynchronousMethod, ICanError
{
    public override string Description => "Spawns an item pickup / grenade at the coordinates.";

    public string[] ErrorReasons =>
    [
        "The projectile/item has already been spawned."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<Pickup>("pickup/projectile reference"),
        new FloatArgument("x position"),
        new FloatArgument("y position"),
        new FloatArgument("z position"),
    ];

    public override void Execute()
    {
        var obj = Args.GetReference<Pickup>("pickup/projectile reference");
        
        Vector3 pos = new(
            Args.GetFloat("x position"),
            Args.GetFloat("y position"),
            Args.GetFloat("z position"));

        SpawnPickup(obj, pos, this);
    }

    // this is here just to make the ErrorReasons universal across all the pickup-spawning methods
    public static SpawnPickupPosMethod Singleton
    {
        get => field is null ? field = new SpawnPickupPosMethod() : field;
    } = null!;

    public static void SpawnPickup(Pickup obj, Vector3 pos, Method caller)
    {
        obj.Position = pos;
        obj.Rotation = Quaternion.identity;
        obj.GameObject.SetActive(true);
        if (NetworkServer.spawned.ContainsValue(obj.NetworkIdentity))
            throw new ScriptRuntimeError(caller, Singleton.ErrorReasons[0]);
        NetworkServer.Spawn(obj.GameObject);
        if (obj is Projectile projectile)
            projectile.Base.ServerActivate();
    }
}