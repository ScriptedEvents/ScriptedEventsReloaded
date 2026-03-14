using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.ElevatorMethods;

[UsedImplicitly]
public class ElevatorInfoMethod : LiteralValueReturningMethod, IReferenceResolvingMethod
{
    public override string Description => IReferenceResolvingMethod.Desc.Get(this);
    public override Argument[] ExpectedArguments => 
        [
            new ReferenceArgument<Elevator>("elevator"),
            new OptionsArgument("info", options:
                [
                    new("name"),
                    new("group"),
                    new("isReady"),
                    new("isGoingUp"),
                    new("currentSequence"),
                    new("allDoorsLockedReason"),
                    new("anyDoorLockedReason"),
                    new("isAdminLocked")
                ])
        ];
    public override void Execute()
    {
        var elevator = Args.GetReference<Elevator>("elevator");
        ReturnValue = Args.GetOption("info") switch
        {
            "name" => new StaticTextValue(elevator.Base.name),
            "group" => new StaticTextValue(elevator.Group.ToString()),
            "isready" => new BoolValue(elevator.IsReady),
            "isgoingup" => new BoolValue(elevator.GoingUp),
            "currentsequence" => new StaticTextValue(elevator.CurrentSequence.ToString()),
            "alldoorslockedreason" => new StaticTextValue(elevator.AllDoorsLockedReason.ToString()),
            "anydoorlockedreason" => new StaticTextValue(elevator.AnyDoorLockedReason.ToString()),
            "isadminlocked" => new BoolValue(elevator.DynamicAdminLock),
            _ => throw new RetroReulFuckedUpException()
        };
    }

    public override TypeOfValue LiteralReturnTypes => new TypesOfValue([
        typeof(TextValue),
        typeof(BoolValue)
    ]);
    public Type ResolvesReference => typeof(Elevator);
}