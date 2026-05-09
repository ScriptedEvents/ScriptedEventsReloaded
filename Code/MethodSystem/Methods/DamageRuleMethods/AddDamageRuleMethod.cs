using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.DamageRuleMethods;

[UsedImplicitly]
public class AddDamageRuleMethod : SynchronousMethod
{
    public override string Description => "Sets the damage rule for a player.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new OptionsArgument("mode", "attacker", "reciever")
        {
            Description = "Indicates whether the damage rule will be applied on all damage dealt by the attacker " +
                          "or on all damage received by the reciever."
        },
        new PlayersArgument("players affected"),
        new FloatArgument("multiplier", preferPercent: true),
        new TextArgument("remove id")
        {
            Description = 
                "The ID of the damage rule. " +
                "This is to identify this specific damage rule if you later want to remove SPECIFICALLY it later. " +
                "There can be multiple rules with the same ID.",
            DefaultValue = new(null, "no rule id")
        },
        new BoolArgument("update")
        {
            Description = 
                "Whether the rule should check for any changes regarding the 'players affected' argument. " +
                "E.g. when you use 'players affected' = ClassD and 'update' = false, " +
                "then the damage rule will NOT update when player roles change, " +
                "meaning that when some ClassD becomes a Guard, " +
                "their damage rule will STILL apply, " +
                "even though they are no longer ClassD.",
            DefaultValue = new(true, null)
        }
    ];

    public override void Execute()
    {
        var players = Args.GetPlayers("players affected");
        var func = Args.GetGetter<Player[], PlayersArgument>("players affected");
        var damageRule = new DamageRuleHandler.DamageRule
        {
            Id = Args.GetText("remove id"),
            Multiplier = Args.GetFloat("multiplier"),
            Getter = Args.GetBool("update")
                ? () => func.Invoke().WasSuccessful(out var value) 
                    ? value 
                    : []
                : () => players
        };
        
        switch (Args.GetOption("mode"))
        {
            case "attacker":
                DamageRuleHandler.AttackerRules.Add(damageRule);
                break;
            case "reciever":
                DamageRuleHandler.RecieverRules.Add(damageRule);
                break;
            default:
                throw new ArgumentException("Invalid mode");
        }
    }
}