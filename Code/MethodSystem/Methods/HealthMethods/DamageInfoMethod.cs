using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using PlayerStatsSystem;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.ArgumentSystem.Structures;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.HealthMethods;

[UsedImplicitly]
public class DamageInfoMethod : ReturningMethod, IReferenceResolvingMethod, IAdditionalDescription
{
    public Type ReferenceType => typeof(DamageHandlerBase);
    
    public override TypeOfValue Returns => new([
        typeof(TextValue), 
        typeof(ReferenceValue)
    ]);
    
    public override string Description => IReferenceResolvingMethod.Desc.Get(this);
    
    public string AdditionalDescription =>
        "A lot of options here might not be available depending on which DamageHandler is used in game. " +
        "It's advised you check every accessed value for 'none' before using it.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<DamageHandlerBase>("handler"),
        new OptionsArgument("property",
            "damage",
            Option.Enum<HitboxType>("hitbox"), 
            Option.Reference<Item>("firearmUsed"), 
            Option.Reference<Player>("attacker")
        )
    ];

    public override void Execute()
    {
        var handler = Args.GetReference<DamageHandlerBase>("handler");
        var standard = handler as StandardDamageHandler;
        var firearm = handler as FirearmDamageHandler;
        var attacker = handler as AttackerDamageHandler;
        
        ReturnValue = Args.GetOption("property") switch
        {
            "damage" => new TextValue(standard?.Damage.ToString() ?? "none"),
            "hitbox" => new TextValue(standard?.Hitbox.ToString() ?? "none"),
            "firearmused" => new ReferenceValue(firearm?.Firearm),
            "attacker" => new ReferenceValue(attacker?.Attacker),
            _ => throw new AndrzejFuckedUpException("out of range")
        };
    }
}