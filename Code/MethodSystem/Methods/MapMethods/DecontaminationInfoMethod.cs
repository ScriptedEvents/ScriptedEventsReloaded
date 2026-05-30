using LabApi.Features.Wrappers;
using LightContainmentZoneDecontamination;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;
using UnityEngine;

namespace SER.Code.MethodSystem.Methods.MapMethods;

[UsedImplicitly]
public class DecontaminationInfoMethod : LiteralValueReturningMethod
{
    public override TypeOfValue LiteralReturnTypes { get; } = new TypesOfValue(
        typeof(BoolValue),
        typeof(DurationValue)
    );
    
    public override string Description => "Returns decontamination info.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new OptionsArgument("option",
            "hasStarted",
            "timeToStart")
    ];

    public override void Execute()
    {
        ReturnValue = Args.GetOption("option") switch
        {
            "hasstarted" => new BoolValue(Decontamination.IsDecontaminating),
            "timetostart" => new DurationValue(
                TimeSpan.FromSeconds(
                    Mathf.Max(
                        0.0f, 
                        DecontaminationController.Singleton.DecontaminationPhases[^1].TimeTrigger 
                        - (float)DecontaminationController.GetServerTime
                    )
                )
            ),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}