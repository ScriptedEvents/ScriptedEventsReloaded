using CustomPlayerEffects;
using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.EffectMethods;

[UsedImplicitly]
public class EffectInfoMethod : LiteralValueReturningMethod, IReferenceResolvingMethod
{
    public override string Description => IReferenceResolvingMethod.Desc.Get(this);

    public override Argument[] ExpectedArguments =>
    [
        new ReferenceArgument<StatusEffectBase>("effect"),
        new OptionsArgument("info", options:
            [
                new("name"),
                new("duration"),
                new("intensity"),
                new("classification"),
                new("timeLeft")
            ])
    ];
    public override void Execute()
    {
        var effect = Args.GetReference<StatusEffectBase>("effect");
        ReturnValue = Args.GetOption("info") switch
        {
            "name" => new StaticTextValue(effect.name),
            "duration" => new DurationValue(TimeSpan.FromSeconds(effect.Duration)),
            "intensity" => new NumberValue(effect.Intensity),
            "classification" => new StaticTextValue(effect.Classification.ToString()),
            "timeleft" => new DurationValue(TimeSpan.FromSeconds(effect.TimeLeft)),
            _ => throw new RetroReulFuckedUpException()
        };
    }

    public override TypeOfValue LiteralReturnTypes => new TypesOfValue([
        typeof(TextValue),
        typeof(NumberValue),
        typeof(DurationValue)
    ]);
    
    public Type ResolvesReference => typeof(StatusEffectBase);
}