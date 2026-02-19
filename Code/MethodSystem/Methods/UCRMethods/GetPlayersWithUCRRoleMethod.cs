using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;
using SER.Code.ValueSystem;
using UncomplicatedCustomRoles.API.Features;

namespace SER.Code.MethodSystem.Methods.UCRMethods;

// ReSharper disable once InconsistentNaming
[UsedImplicitly]
public class GetPlayersWithUCRRoleMethod : ReturningMethod<PlayerValue>, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.Ucr;

    public override string Description => "Gets all players who have a provided UCR role.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new IntArgument("role id")
    ];

    public override void Execute()
    {
        var id = Args.GetInt("role id");
        ReturnValue = Player.ReadyList
            .Where(p => SummonedCustomRole.Get(p)?.Role.Id == id)
            .ToPlayerValue();
    }
}