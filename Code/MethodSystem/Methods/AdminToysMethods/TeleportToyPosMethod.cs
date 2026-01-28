using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using Mirror;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using UnityEngine;

namespace SER.Code.MethodSystem.Methods.AdminToysMethods;

[UsedImplicitly]
public class TeleportToyPosMethod : SynchronousMethod, IAdditionalDescription
{
    public override string Description => "Teleports an Admin Toy to the given absolute coordinates";

    public string AdditionalDescription => "Spawns the toy if it hasn't been already";

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<AdminToy>("toy reference"),
        new OptionsArgument("mode",
            "add",
            "set")
        {
            Description = "Indicates whether to add the vector to the toy's position" +
                          " or to set the position to it."
        },
        
        new FloatArgument("x position"),
        new FloatArgument("y position"),
        new FloatArgument("z position"),
    ];

    public override void Execute()
    {
        var toy = Args.GetReference<AdminToy>("toy reference");
        var mode = Args.GetOption("mode");
        var pos = new Vector3(
            Args.GetFloat("x position"),
            Args.GetFloat("y position"),
            Args.GetFloat("z position"));
        
        TeleportOrSpawn(toy, mode == "add" ? toy.Position + pos : pos);
    }

    // Once again the singleton is there just to make the ErrorReasons universal
    public static TeleportToyPosMethod Singleton
    {
        get => field is null ? field = new TeleportToyPosMethod() : field;
    } = null!;

    public static void TeleportOrSpawn(AdminToy toy, Vector3 position, Quaternion? rotation = null)
    {
        if (rotation is not null)
        {
            toy.Rotation = rotation.Value;
        }
        toy.Position = position;
        toy.GameObject.SetActive(true);
        if (!NetworkServer.spawned.ContainsValue(toy.GameObject.NetworkIdentity))
            toy.Spawn();
    }
}