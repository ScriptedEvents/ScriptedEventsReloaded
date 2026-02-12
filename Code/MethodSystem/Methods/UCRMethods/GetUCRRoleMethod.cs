using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.MethodSystem.Methods.ReferenceVariableMethods;
using SER.Code.MethodSystem.Structures;
using SER.Code.ValueSystem;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;

namespace SER.Code.MethodSystem.Methods.UCRMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class GetUCRRoleMethod : ReferenceReturningMethod, IAdditionalDescription, IDependOnFramework
{
    public IDependOnFramework.Type DependsOn => IDependOnFramework.Type.Ucr;
    
    public override string Description => "Returns a reference to the UCR role a player has.";

    public string AdditionalDescription =>
        $"Be sure to use {GetFriendlyName(typeof(ValidRefMethod))} method to verify if the player has a role. " +
        $"The reference will be INVALID when the player doesn't have a role.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayerArgument("player")
    ];
    
    public override void Execute()
    {
        ReturnValue = new ReferenceValue(SummonedCustomRole.Get(Args.GetPlayer("player"))?.Role!);
    }

    public override Type ReturnType { get; } = typeof(ICustomRole);
}