using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;

namespace SER.Code.MethodSystem.Methods.ElevatorMethods;

[UsedImplicitly]
public class SetElevatorTextMethod : SynchronousMethod, IAdditionalDescription
{
    public override string Description => "Changes the text on the elevator panels between LCZ and HCZ.";

    public override Argument[] ExpectedArguments =>
    [
        new TextArgument("text")
        {
            DefaultValue = new(string.Empty, "Resets the text to it's original value."),
        }
    ];
    
    public override void Execute()
    {
        Decontamination.ElevatorsText = Args.GetText("text");
    }

    public string AdditionalDescription => "An empty text value will reset the elevator panel text to it's original value.";
}