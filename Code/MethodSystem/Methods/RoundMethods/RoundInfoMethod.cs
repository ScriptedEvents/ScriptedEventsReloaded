using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.RoundMethods;

[UsedImplicitly]
public class RoundInfoMethod : LiteralValueReturningMethod
{
    public override string Description => "Returns information about the current round.";
    
    public override TypeOfValue LiteralReturnTypes => new TypesOfValue([
        typeof(BoolValue), 
        typeof(DurationValue)
    ]);

    public override Argument[] ExpectedArguments { get; } =
    [
        new OptionsArgument("mode",
            "hasStarted",
            "isInProgress",
            "hasEnded",
            "duration"
        )
    ];

    public override void Execute()
    {
        ReturnValue = Args.GetOption("mode") switch
        {
            "hasstarted" => new BoolValue(Round.IsRoundStarted),
            "isinprogress" => new BoolValue(Round.IsRoundInProgress),
            "hasended" => new BoolValue(Round.IsRoundEnded),
            "duration" => new DurationValue(Round.Duration),
            _ => throw new AndrzejFuckedUpException()
        };
    }
}