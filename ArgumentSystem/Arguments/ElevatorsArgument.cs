using Interactables.Interobjects;
using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.ArgumentSystem.BaseArguments;
using SER.Helpers.ResultSystem;
using SER.TokenSystem.Tokens;

namespace SER.ArgumentSystem.Arguments;

public class ElevatorsArgument(string name) : EnumHandlingArgument(name)
{
    public override string InputDescription => $"{nameof(ElevatorGroup)} enum or * for every elevator";

    [UsedImplicitly]
    public DynamicTryGet<Elevator[]> GetConvertSolution(BaseToken token)
    {
        return ResolveEnums<Elevator[]>(
            token,
            new()
            {
                [typeof(ElevatorGroup)] = group =>
                    Elevator.List.Where(elevator => elevator.Group == (ElevatorGroup)group).ToArray(),
            },
            () =>
            {
                if (token is SymbolToken { IsJoker: true })
                {
                    return Elevator.List.ToArray();
                }

                return
                    $"Value '{token.RawRep}' cannot be interpreted as an elevator or collection of elevators.";
            }
        );
    }
}