using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.RespawnMethods;

[UsedImplicitly]
public class GetWaveTimerMethod : ReturningMethod<DurationValue>, IAdditionalDescription
{
    public override string Description => "Returns the duration of a given spawn wave.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new WaveTypeArgument("wave type")
    ];
    
    public override void Execute()
    {
        ReturnValue = TimeSpan.FromSeconds(Args.GetWaveType("wave type") switch
        {
            var t when t == typeof(MtfWave) => RespawnWaves.PrimaryMtfWave?.TimeLeft ?? 0,
            var t when t == typeof(MiniMtfWave) => RespawnWaves.MiniMtfWave?.TimeLeft ?? 0,
            var t when t == typeof(ChaosWave) => RespawnWaves.PrimaryChaosWave?.TimeLeft ?? 0,
            var t when t == typeof(MiniChaosWave) => RespawnWaves.MiniChaosWave?.TimeLeft ?? 0,
            _ => throw new AndrzejFuckedUpException()
        });
    }

    public string AdditionalDescription => "Will return 0s if the wave is not active.";
}