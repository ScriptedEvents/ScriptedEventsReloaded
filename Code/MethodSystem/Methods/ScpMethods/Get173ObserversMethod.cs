using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using PlayerRoles.PlayableScps.Scp173;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.ScpMethods;

[UsedImplicitly]
public class Get173ObserversMethod : ReturningMethod<PlayerValue>, ICanError
{
    public override string Description => "Returns the people that are looking at the specified SCP-173";

    public string[] ErrorReasons =>
    [
        "The specified player isn't SCP-173."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayerArgument("peanut")
        {
            DefaultValue = new(null, "Every observer from every SCP-173")
        }
    ];

    public override void Execute()
    {
        var peanut = Args.GetPlayer("peanut").MaybeNull();

        if (peanut is null)
        {
            ReturnValue = new PlayerValue(Player.ReadyList
                .Select(plr => plr.RoleBase is Scp173Role sculpture ? sculpture.ObservingPlayers : null)
                .RemoveNulls()
                .Flatten());
            return;
        }
        
        if (peanut.RoleBase is not Scp173Role pnutRole)
            throw new ScriptRuntimeError(this, ErrorReasons[0]);
        
        ReturnValue = new PlayerValue(pnutRole.ObservingPlayers);
    }
}