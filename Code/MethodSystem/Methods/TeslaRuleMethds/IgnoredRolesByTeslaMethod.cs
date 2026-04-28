using PlayerRoles;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;

namespace SER.Code.MethodSystem.Methods.TeslaRuleMethds;

[UsedImplicitly]
public class IgnoredRolesByTeslaMethod : SynchronousMethod, IAdditionalDescription
{
    public override string Description => "Sets the type of roles that will be ignored by a tesla.";

    public string AdditionalDescription => "The list of ignored roles is reset for every round.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new OptionsArgument(
            "mode", 
            new("set", "Sets the list, overriding previous values."),
            new("add", "Adds new roles to the list."), 
            new("remove", "Removes roles from the list, making them be triggering a tesla.")
        ),
        new EnumArgument<RoleTypeId>("roles")
        {
            ConsumesRemainingValues = true
        }
    ];

    public override void Execute()
    {
        var roles = Args.GetRemainingArguments<RoleTypeId, EnumArgument<RoleTypeId>>("roles");
        
        switch (Args.GetOption("mode"))
        {
            case "set":
                TeslaRuleHandler.IgnoredRoles = roles.ToHashSet();
                return;
            case "add":
                TeslaRuleHandler.IgnoredRoles.UnionWith(roles);
                return;
            case "remove":
                TeslaRuleHandler.IgnoredRoles.ExceptWith(roles);
                return;
        }
    }
}