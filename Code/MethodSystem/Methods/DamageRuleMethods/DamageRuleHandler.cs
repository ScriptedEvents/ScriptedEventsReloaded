using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using PlayerStatsSystem;

namespace SER.Code.MethodSystem.Methods.DamageRuleMethods;

public class DamageRuleHandler : CustomEventsHandler
{
    public struct DamageRule
    {
        public required string? Id;
        public required float Multiplier;
        public required Func<Player[]> Getter;
    }
    
    public static readonly List<DamageRule> AttackerRules = [];
    public static readonly List<DamageRule> ReceiverRules = [];

    public static void ResetAll()
    {
        AttackerRules.Clear();
        ReceiverRules.Clear();
    }

    public static void RemoveRule(string? id)
    {
        if (id is null)
        {
            ResetAll();
            return;
        }
        
        AttackerRules.RemoveAll(rule => rule.Id == id);
        ReceiverRules.RemoveAll(rule => rule.Id == id);
    }

    public override void OnPlayerHurting(PlayerHurtingEventArgs ev)
    {
        if (ev.DamageHandler is not StandardDamageHandler handler) return;
        
        if (ev.Player is { } receiver)
        {
            Apply(ReceiverRules, receiver);
        }
        
        if (ev.Attacker is { } attacker)
        {
            Apply(AttackerRules, attacker);
        }

        return;

        void Apply(List<DamageRule> rules, Player player)
        {
            foreach (var rule in rules)
            {
                if (rule.Getter().Contains(player))
                {
                    handler.Damage *= rule.Multiplier;
                }
            }
        }
    }
}
