using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using PlayerRoles;
using Respawning;
using Respawning.Waves.Generic;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.ArgumentSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.RespawnMethods;

[UsedImplicitly]
public class RespawnWaveInfoMethod : LiteralValueReturningMethod, IReferenceResolvingMethod
{
    public Type ResolvesReference => typeof(RespawnWave);

    public override TypeOfValue LiteralReturnTypes => new TypesOfValue([
        typeof(NumberValue), 
        typeof(TextValue),
        typeof(DurationValue)
    ]);

    public override string Description => IReferenceResolvingMethod.Desc.Get(this);

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<RespawnWave>("respawnWave"),
        new OptionsArgument("property", 
            Option.Enum<Faction>(),
            "maxWaveSize",
            "respawnTokens",
            "influence",
            "timeLeft"
        )
    ];

    public override void Execute()
    {
        var wave = Args.GetReference<RespawnWave>("respawnWave");

        ReturnValue = Args.GetOption("property") switch
        {
            "faction" => new StaticTextValue(wave.Faction.ToString()),
            "maxwavesize" => new NumberValue(wave.MaxWaveSize),
            "respawntokens" => new NumberValue(wave.Base is ILimitedWave limitedWave ? limitedWave.RespawnTokens : 0),
            "influence" => new NumberValue((decimal)FactionInfluenceManager.Get(wave.Faction)),
            "timeleft" => new DurationValue(TimeSpan.FromSeconds(wave.TimeLeft)),
            _ => throw new AndrzejFuckedUpException()
        };
    }
}