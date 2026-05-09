using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.TeslaRuleMethods;

[UsedImplicitly]
public class AddTeslaIgnoreRuleMethod : SynchronousMethod
{
    public override string Description => "Sets the players that will be ignored by a tesla.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new TextArgument("remove id")
        {
            Description = 
                "The ID of the tesla ignore rule. " +
                "This is to identify this specific tesla ignore rule if you later want to remove SPECIFICALLY it later. " +
                "There can be multiple tesla ignore rules with the same ID.",
            DefaultValue = new(null, "no rule id")
        },
        new BoolArgument("update")
        {
            Description = 
                "Whether the rule should check for any changes regarding the 'players' argument. " +
                "E.g. when you use 'players' = ClassD and 'update' = false, " +
                "then the rule will NOT update when player roles change, " +
                "meaning that when some ClassD becomes a Guard, " +
                "their rule will STILL apply, " +
                "even though they are no longer ClassD.",
            DefaultValue = new(true, null)
        }
    ];

    public override void Execute()
    {
        Func<Player[]> getter;
        if (Args.GetBool("update"))
        {
            var func = Args.GetGetter<Player[], PlayersArgument>("players");
            getter = () => func.Invoke()
                    .WasSuccessful(out var players)
                    ? players
                    : [];
        }
        else
        {
            var players = Args.GetPlayers("players");
            getter = () => players;
        }
        
        TeslaRuleHandler.Rules.Add(new()
        {
            Id = Args.GetText("remove id"),
            Players = getter
        });
    }
}
