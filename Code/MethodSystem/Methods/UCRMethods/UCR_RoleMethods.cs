using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers;
using SER.Code.Integrations.Ucr;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;
using SER.Code.ValueSystem;

// ReSharper disable InconsistentNaming
namespace SER.Code.MethodSystem.Methods.UCRMethods;

[UsedImplicitly]
public class UCR_GetRolesMethod : ReturningMethod<CollectionValue<ReferenceValue<UCRRole>>>, IAdditionalDescription, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.UncomplicatedCustomRoles;
    public override string Description => "Lists registered UCR roles.";
    public string AdditionalDescription => "Each reference exposes id, name, and object. UCR role properties are available through the same reference.";
    public override Argument[] ExpectedArguments { get; } = [];
    public override void Execute() => ReturnValue = new CollectionValue<ReferenceValue<UCRRole>>(UcrBridge.GetRoles());
}

[UsedImplicitly]
public class UCR_GetRoleByIdMethod : ReferenceReturningMethod<UCRRole>, IAdditionalDescription, ICanError, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.UncomplicatedCustomRoles;
    public override string Description => "Gets a registered UCR role by ID.";
    public string AdditionalDescription => "The reference exposes id, name, and object. UCR role properties are available through the same reference.";
    public override Argument[] ExpectedArguments { get; } = [new IntArgument("role id")];
    public string[] ErrorReasons => ["The role ID is not registered in UCR."];
    public override void Execute() => ReturnValue = UcrBridge.GetRole(Args.GetInt("role id"));
}

[UsedImplicitly]
public class UCR_GetRoleByNameMethod : ReferenceReturningMethod<UCRRole>, IAdditionalDescription, ICanError, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.UncomplicatedCustomRoles;
    public override string Description => "Gets a registered UCR role by name.";
    public string AdditionalDescription => "Role names are matched without case sensitivity.";
    public override Argument[] ExpectedArguments { get; } = [new TextArgument("role name")];
    public string[] ErrorReasons => ["The role name is not registered in UCR."];
    public override void Execute() => ReturnValue = UcrBridge.GetRole(Args.GetText("role name"));
}

[UsedImplicitly]
public class UCR_GetRoleInstanceMethod : ReferenceReturningMethod<UCRRoleInstance>, IAdditionalDescription, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.UncomplicatedCustomRoles;
    public override string Description => "Gets a player's active UCR role instance.";
    public string AdditionalDescription => "Returns an invalid reference when the player does not have a UCR role. The reference exposes id, player, role, and object.";
    public override Argument[] ExpectedArguments { get; } = [new PlayerArgument("player")];
    public override void Execute() => ReturnValue = UcrBridge.GetRoleInstance(Args.GetPlayer("player"))!;
}

[UsedImplicitly]
public class UCR_GetRoleInstancesMethod : ReturningMethod<CollectionValue<ReferenceValue<UCRRoleInstance>>>, IAdditionalDescription, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.UncomplicatedCustomRoles;
    public override string Description => "Lists active instances of a UCR role.";
    public string AdditionalDescription => "Each reference exposes id, player, role, and object.";
    public override Argument[] ExpectedArguments { get; } = [new ReferenceArgument<UCRRole>("role reference")];
    public override void Execute() => ReturnValue = new CollectionValue<ReferenceValue<UCRRoleInstance>>(
        UcrBridge.GetRoleInstances(Args.GetReference<UCRRole>("role reference")));
}

[UsedImplicitly]
public class UCR_GetSpawnedCountMethod : ReturningMethod<NumberValue>, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.UncomplicatedCustomRoles;
    public override string Description => "Counts players using a UCR role.";
    public override Argument[] ExpectedArguments { get; } = [new ReferenceArgument<UCRRole>("role reference")];
    public override void Execute() => ReturnValue = UcrBridge.GetSpawnedCount(Args.GetReference<UCRRole>("role reference"));
}

[UsedImplicitly]
public class UCR_IsRegisteredMethod : ReturningMethod<BoolValue>, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.UncomplicatedCustomRoles;
    public override string Description => "Checks whether a UCR role is still registered.";
    public override Argument[] ExpectedArguments { get; } = [new ReferenceArgument<UCRRole>("role reference")];
    public override void Execute() => ReturnValue = UcrBridge.IsRegistered(Args.GetReference<UCRRole>("role reference"));
}

[UsedImplicitly]
public class UCR_RemoveRoleMethod : ReturningMethod<BoolValue>, IAdditionalDescription, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.UncomplicatedCustomRoles;
    public override string Description => "Removes a player's UCR role.";
    public string AdditionalDescription => "Returns false when the player did not have a UCR role. Enable reset role to discard health and other changes made by the role.";
    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayerArgument("player"),
        new BoolArgument("reset role") { DefaultValue = new(false, "keep the current base role") }
    ];

    public override void Execute() => ReturnValue = UcrBridge.RemoveRole(
        Args.GetPlayer("player"),
        Args.GetBool("reset role"));
}
