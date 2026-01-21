using Exiled.Events.Handlers;
using JetBrains.Annotations;
using PlayerRoles.PlayableScps.Scp173;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.Extensions;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;
using Player = LabApi.Features.Wrappers.Player;

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
    ];

    public override void Execute()
    {
        var pnut = Args.GetPlayer("peanut");
        
        if (pnut.RoleBase is not Scp173Role pnutRole)
            throw new ScriptRuntimeError(this, ErrorReasons[0]);
        
        ReturnValue = new PlayerValue(pnutRole.ObservingPlayers);
    }
}