using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.RespawnMethods;

[UsedImplicitly]
public class GetWaveTimerMethod : ReturningMethod<DurationValue>, IAdditionalDescription
{
    public override string Description => "Returns the duration of a given spawn wave.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new WaveArgument("wave type")
    ];
    
    public override void Execute()
    {
        ReturnValue = TimeSpan.FromSeconds(Args.GetWave("wave type") switch
        {
            MtfWave => RespawnWaves.PrimaryMtfWave?.TimeLeft is {} wave and > 0 ? wave : 0,
            MiniMtfWave => RespawnWaves.MiniMtfWave?.TimeLeft is {} wave and > 0 ? wave : 0,
            ChaosWave => RespawnWaves.PrimaryChaosWave?.TimeLeft is {} wave and > 0 ? wave : 0,
            MiniChaosWave => RespawnWaves.MiniChaosWave?.TimeLeft is {} wave and > 0 ? wave : 0,
            _ => throw new AndrzejFuckedUpException()
        });
    }

    public string AdditionalDescription => "Will return 0s if the wave is not active or spawning.";
}