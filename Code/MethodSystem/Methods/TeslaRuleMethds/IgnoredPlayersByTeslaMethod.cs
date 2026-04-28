using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;

namespace SER.Code.MethodSystem.Methods.TeslaRuleMethds;

[UsedImplicitly]
public class IgnoredPlayersByTeslaMethod : SynchronousMethod, IAdditionalDescription
{
    public override string Description => "Sets the players that will be ignored by a tesla.";

    public string AdditionalDescription => 
        "The list of ignored players is reset for every round. " +
        "IMPORTANT: 'IgnoredPlayersByTesla set @all' and similar will NOT update the list of ignored players when a new player joins, " +
        "because when 'IgnoredPlayersByTesla' was used, the @all variable did not contain the newly joined player. " +
        "This has to be accounted for manually: 'IgnoredPlayersByTesla add @newlyJoinedPlayer'";

    public override Argument[] ExpectedArguments { get; } =
    [
        new OptionsArgument(
            "mode", 
            new("set", "Sets the list, overriding previous values."),
            new("add", "Adds new players to the list."), 
            new("remove", "Removes players from the list, making them be triggering a tesla.")
        ),
        new PlayersArgument("players")
    ];

    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var ids = players.Select(p => p.PlayerId);
        
        switch (Args.GetOption("mode"))
        {
            case "set":
                TeslaRuleHandler.IgnoredPlayerIds = ids.ToHashSet();
                return;
            case "add":
                TeslaRuleHandler.IgnoredPlayerIds.UnionWith(ids);
                return;
            case "remove":
                TeslaRuleHandler.IgnoredPlayerIds.ExceptWith(ids);
                return;
        }
    }
}
