using LabApi.Features.Enums;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.Arguments;

public class GateArgument(string name) : EnumHandlingArgument(name)
{
    public override string InputDescription => 
        $"{nameof(DoorName)} enum (that is a gate) " +
        $"or reference to {nameof(Gate)}";

    [UsedImplicitly]
    public DynamicTryGet<Gate> GetConvertSolution(BaseToken token)
    {
        return ValueOrEnumResolver<Gate>(token, value =>
        {
            return value is ReferenceValue reference
                ? reference.GetAs<Gate>()
                : GenericError(token);
        }, [
            new EnumHandler<DoorName, Gate>(doorName => new(() =>
            {
                return Gate.List
                    .Where(gate => gate.DoorName == doorName)
                    .TryGetRandomValue($"Gate with name '{doorName}' does not exist.");
            }))]);
    }
}
