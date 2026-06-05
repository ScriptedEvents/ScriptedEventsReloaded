using LabApi.Features.Enums;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ArgumentSystem.Arguments;

public class GateArgument(string name) : EnumHandlingArgument(name)
{
    public override string InputDescription => 
        $"{nameof(DoorName)} enum (that is a gate) " +
        $"or reference to {nameof(Gate)}";

    [UsedImplicitly]
    public DynamicTryGet<Gate> GetConvertSolution(BaseToken token)
    {
        if (token.CanReturnReference<Gate>(out var func))
        {
            return func;
        }
        
        return EnumResolver<Gate>(token, [
            new EnumHandler<DoorName,Gate>(doorName =>
            {
                var door = Gate.List
                    .Where(gate => gate.DoorName == doorName)
                    .GetRandomValue();
                if (door is null)
                {
                    return $"Gate with name '{doorName}' does not exist.";
                }

                return door;
            })]
        );
    }
}