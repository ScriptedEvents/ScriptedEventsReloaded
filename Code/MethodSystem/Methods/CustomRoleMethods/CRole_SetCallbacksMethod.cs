using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Methods.CustomRoleMethods.Structures;
using SER.Code.ScriptSystem.Structures;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.CustomRoleMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class CRole_SetCallbacksMethod : SynchronousMethod
{
    public override string Description =>
        "Sets the callbacks for a provided custom role. Calling it again from the same script replaces that role's callbacks without changing callbacks for other roles.";

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
            CRole.AddOrReplaceHandler(CRole.CustomRoleEvent.Spawned, GetHandler(onSpawning));
        }
        
        if (Args.GetCallback("on removing") is { } onRemoving)
        {
            CRole.AddOrReplaceHandler(CRole.CustomRoleEvent.Removed, GetHandler(onRemoving));
        }

        return;

        CRole.Handler GetHandler(CallbackArgument.Callback callback)
        {
            return new()
            {
                Action = (plr, role) => callback.Action([new PlayerValue(plr), new ReferenceValue<CRole>(role)], null),
                Id = GetHandlerId(customRole.Id, Script.Name),
                ForRoles = [customRole.Id]
            };
        }
    }

    internal static string GetHandlerId(string roleId, ScriptName scriptName) =>
        $"callbacks for custom role '{roleId}' in script '{scriptName}'";
}
