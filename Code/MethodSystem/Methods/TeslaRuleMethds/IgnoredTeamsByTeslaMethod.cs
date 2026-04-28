using PlayerRoles;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;

namespace SER.Code.MethodSystem.Methods.TeslaRuleMethds;

[UsedImplicitly]
public class IgnoredTeamsByTeslaMethod : SynchronousMethod, IAdditionalDescription
{
    public override string Description => "Sets the type of teams that will be ignored by a tesla.";

    public string AdditionalDescription => "The list of ignored teams is reset for every round.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new OptionsArgument(
            "mode", 
            new("set", "Sets the list, overriding previous values."),
            new("add", "Adds new teams to the list."), 
            new("remove", "Removes teams from the list, making them be triggering a tesla.")
        ),
        new EnumArgument<Team>("teams")
        {
            ConsumesRemainingValues = true
        }
    ];

    public override void Execute()
    {
        var teams = Args.GetRemainingArguments<Team, EnumArgument<Team>>("teams");
        
        switch (Args.GetOption("mode"))
        {
            case "set":
                TeslaRuleHandler.IgnoredTeams = teams.ToHashSet();
                return;
            case "add":
                TeslaRuleHandler.IgnoredTeams.UnionWith(teams);
                return;
            case "remove":
                TeslaRuleHandler.IgnoredTeams.ExceptWith(teams);
                return;
        }
    }
}