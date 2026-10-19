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
        "Sets the callbacks for a provided custom role. Calling it again from the same script replaces " +
        "both of that role's callbacks: a callback you leave out is turned off. " +
        "Callbacks for other roles and from other scripts stay as they are.";

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
        var handlerId = GetHandlerId(customRole.Id, Script.Name);

        // Both callbacks are replaced on every call. An omitted callback removes the one
        // this script installed earlier for this role, so it stops running.
        SetOrRemove(CRole.CustomRoleEvent.Spawned, Args.GetCallback("on spawning"));
        SetOrRemove(CRole.CustomRoleEvent.Removed, Args.GetCallback("on removing"));

        return;

        void SetOrRemove(CRole.CustomRoleEvent @event, CallbackArgument.Callback? callback)
        {
            if (callback is null)
            {
                CRole.RemoveHandler(@event, handlerId);
                return;
            }

            CRole.AddOrReplaceHandler(@event, new()
            {
                Action = (plr, role) => callback.Action([new PlayerValue(plr), new ReferenceValue<CRole>(role)], null),
                Id = handlerId,
                ForRoles = [customRole.Id]
            });
        }
    }

    internal static string GetHandlerId(string roleId, ScriptName scriptName) =>
        $"callbacks for custom role '{roleId}' in script '{scriptName}'";
}
