using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Methods.CustomRoleMethods.Structures;
using SER.Code.ScriptSystem.Structures;
using SER.Code.ValueSystem;
using YamlDotNet.Serialization.NodeDeserializers;

namespace SER.Code.MethodSystem.Methods.CustomRoleMethods;

[UsedImplicitly]
public class SetCRoleCallbacksMethod : SynchronousMethod
{
    public override string Description => "Sets the callbacks for a provided custom role.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new CustomRoleArgument("custom role"),
        new CallbackArgument("on spawning", 
            (typeof(PlayerValue), "player"),
            (typeof(ReferenceValue<CRole>), "role"))
        {
            Description = "This will be called when a player is being spawned with this role.",
            DefaultValue = new(null, "no spawning callback")
        },
        new CallbackArgument("on removing", 
            (typeof(PlayerValue), "player"),
            (typeof(ReferenceValue<CRole>), "role"))
        {
            Description = "This will be called when this role is being taken away from a player.",
            DefaultValue = new(null, "no removing callback")
        }
    ];
        
    public override void Execute()
    {
        var customRole = Args.GetCustomRole("custom role");

        if (Args.GetCallback("on spawning") is { } onSpawning)
        {
            CRole.EventHandlers.AddOrInitListWithKey(CRole.CustomRoleEvent.Spawned, GetHandler(onSpawning));
        }
        
        if (Args.GetCallback("on removing") is { } onRemoving)
        {
            CRole.EventHandlers.AddOrInitListWithKey(CRole.CustomRoleEvent.Removed, GetHandler(onRemoving));
        }

        return;

        CRole.Handler GetHandler(CallbackArgument.Callback callback)
        {
            return new()
            {
                Action = (plr, role) => callback.Action([new PlayerValue(plr), new ReferenceValue<CRole>(role)], null),
                Id = $"callback '{callback.Name}' in script '{Script.Name}'",
                ForRoles = [customRole.Id]
            };
        }
    }
}