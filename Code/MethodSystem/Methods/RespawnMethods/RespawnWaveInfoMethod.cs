using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using PlayerRoles;
using Respawning;
using Respawning.Waves;
using Respawning.Waves.Generic;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.ArgumentSystem.Structures;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.RespawnMethods;

[UsedImplicitly]
public class RespawnWaveInfoMethod : LiteralValueReturningMethod, IReferenceResolvingMethod
{
    public Type ReferenceType => typeof(TimeBasedWave);

    public override TypeOfValue LiteralReturnTypes => new TypesOfValue([
        typeof(NumberValue), 
        typeof(TextValue),
        typeof(DurationValue)
    ]);

    public override string Description => IReferenceResolvingMethod.Desc.Get(this);

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<TimeBasedWave>("respawnWave"),
        new OptionsArgument("property", 
            Option.Enum<Faction>(),
            "maxWaveSize",
            "respawnTokens",
            "influence",
            "secondsLeft"
        )
    ];

    public override void Execute()
    {
        var wave = Args.GetReference<TimeBasedWave>("respawnWave");

        ReturnValue = Args.GetOption("property") switch
        {
            "faction" => new TextValue(wave.TargetFaction.ToString()),
            "maxwavesize" => new NumberValue(wave.MaxWaveSize),
            "respawntokens" => new NumberValue(wave is ILimitedWave limitedWave ? limitedWave.RespawnTokens : 0),
            "influence" => new NumberValue((decimal)FactionInfluenceManager.Get(wave.TargetFaction)),
            "timeleft" => new DurationValue(TimeSpan.FromSeconds(wave.Timer.TimeLeft)),
            _ => throw new AndrzejFuckedUpException()
        };
    }
}