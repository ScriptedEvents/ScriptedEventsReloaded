using Interactables.Interobjects;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.Arguments;

public class ElevatorsArgument(string name) : EnumHandlingArgument(name)
{
    public override string InputDescription => 
        $"{nameof(ElevatorGroup)} enum, " +
        $"reference to an elevator " +
        $"or 'all' for every elevator";

    [UsedImplicitly]
    public DynamicTryGet<Elevator[]> GetConvertSolution(BaseToken token)
    {
        if (token is SymbolToken { IsJoker: true } or AllToken) 
        {
            return new(() => Elevator.List.ToArray());
        }

        return ValueOrEnumResolver<Elevator[]>(token, value =>
        {
            return value is ReferenceValue reference
                ? reference.GetAs<Elevator>().OnSuccess<Elevator[]>(elevator => [elevator])
                : GenericError(token);
        }, [
            new EnumHandler<ElevatorGroup, Elevator[]>(group => new(() =>
            {
                return Elevator.List
                    .Where(elevator => elevator.Group == group)
                    .ToArray();
            }))]);
    }
}
