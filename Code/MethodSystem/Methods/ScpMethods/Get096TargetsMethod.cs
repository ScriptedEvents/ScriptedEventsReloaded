using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using PlayerRoles.PlayableScps.Scp096;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.ScpMethods;

[UsedImplicitly]
public class Get096TargetsMethod : ReturningMethod<PlayerValue>, ICanError
{
    public override string Description => "Returns the targets of the specified SCP-096";

    public string[] ErrorReasons =>
    [
        "The specified player isn't SCP-096."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayerArgument("shy guy")
        {
            DefaultValue = new(null, "Every target from every SCP-096")
        }
    ];

    public override void Execute()
    {
        var shyGuy = Args.GetPlayer("shy guy").MaybeNull();

        if (shyGuy is null)
        {
            ReturnValue = new PlayerValue(Player.ReadyList
                .Select(plr => plr.RoleBase is Scp096Role paleGuy ? paleGuy.Targets : null)
                .RemoveNulls()
                .Flatten());
            return;
        }
        
        if (shyGuy.RoleBase is not Scp096Role shyGuyRole)
            throw new ScriptRuntimeError(this, ErrorReasons[0]);
        
        ReturnValue = new PlayerValue(shyGuyRole.Targets);
    }
}