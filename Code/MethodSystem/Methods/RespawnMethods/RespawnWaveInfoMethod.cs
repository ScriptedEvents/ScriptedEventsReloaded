using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using PlayerRoles;
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
    public Type ReferenceType => typeof(RespawnWave);

    public override TypeOfValue LiteralReturnTypes => new TypesOfValue([
        typeof(NumberValue), 
        typeof(TextValue)
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
            "secondsLeft")
    ];

    public override void Execute()
    {
        var wave = Args.GetReference<RespawnWave>("respawnWave");
        ReturnValue = Args.GetOption("property") switch
        {
            "faction" => new TextValue(wave.Faction.ToString()),
            "maxwavesize" => new NumberValue(wave.MaxWaveSize),
            "respawntokens" => new NumberValue(wave.RespawnTokens),
            "influence" => new NumberValue((decimal)wave.Influence),
            "secondsleft" => new NumberValue((decimal)wave.TimeLeft),
            _ => throw new AndrzejFuckedUpException()
        };
    }
}