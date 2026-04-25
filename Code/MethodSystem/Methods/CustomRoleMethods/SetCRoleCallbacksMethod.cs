using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Methods.CustomRoleMethods.Structures;
using SER.Code.ValueSystem;

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
        customRole.SpawnAction = args => Args.GetCallback("on spawning").Action(args, null);
        customRole.RemoveAction = args => Args.GetCallback("on removing").Action(args, null);
    }
}