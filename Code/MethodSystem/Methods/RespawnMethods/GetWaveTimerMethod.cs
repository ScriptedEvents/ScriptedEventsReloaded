using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.RespawnMethods;

[UsedImplicitly]
public class GetWaveTimerMethod : ReferenceReturningMethod<Result<DurationValue>>
{
    public override string Description => "Returns the duration of a given spawn wave.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new OptionsArgument("wave type",
            "primaryMtfWave",
            "miniMtfWave",
            "primaryChaosWave",
            "miniChaosWave"
        )
    ];
    
    public override void Execute()
    {
        float duration = Args.GetOption("wave type") switch
        {
            "primarymtfwave" => RespawnWaves.PrimaryMtfWave?.TimeLeft ?? -1,
            "minimtfwave" => RespawnWaves.MiniMtfWave?.TimeLeft ?? -1,
            "primarychaoswave" => RespawnWaves.PrimaryChaosWave?.TimeLeft ?? -1,
            "minichaoswave" => RespawnWaves.MiniChaosWave?.TimeLeft ?? -1,
            _ => throw new AndrzejFuckedUpException()
        };

        ReturnValue = new Result<DurationValue>(
            duration >= 0
                ? TimeSpan.FromSeconds(duration)
                : null
        );
    }
}