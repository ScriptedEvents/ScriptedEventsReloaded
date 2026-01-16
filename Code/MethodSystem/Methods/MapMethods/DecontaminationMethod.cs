using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using LightContainmentZoneDecontamination;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods;

namespace SER.Code.MethodSystem.Methods.MapMethods;

[UsedImplicitly]
public class DecontaminationMethod : SynchronousMethod
{
    public override string Description => "Controls the LCZ decontamination.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new OptionsArgument("mode",
            "enable",
            "disable",
            "force"
        )
    ];
    
    public override void Execute()
    {
        Decontamination.Status = Args.GetOption("mode") switch
        {
            "enable" => DecontaminationController.DecontaminationStatus.None,
            "disable" => DecontaminationController.DecontaminationStatus.Disabled,
            "force" => DecontaminationController.DecontaminationStatus.Forced,
            _ => throw new AndrzejFuckedUpException()
        };
    }
}