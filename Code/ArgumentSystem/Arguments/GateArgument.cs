using LabApi.Features.Enums;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.Arguments;

public class GateArgument(string name) : EnumHandlingArgument(name)
{
    public override string InputDescription => $"{nameof(DoorName)} enum (that is a gate) or reference to {nameof(Gate)}";

    [UsedImplicitly]
    public DynamicTryGet<Gate> GetConvertSolution(BaseToken token)
    {
        return ResolveEnums<Gate>(
            token,
            new()
            {
                [typeof(DoorName)] = doorName =>
                {
                    var door = Gate.List.Where(gate => gate.DoorName == (DoorName)doorName).GetRandomValue();
                    if (door is null)
                    {
                        return $"Gate with name '{doorName}' does not exist.";
                    }   

                    return door;
                }
            },
            () =>
            {
                Result rs = $"Value '{token.RawRep}' cannot be interpreted as {InputDescription}.";
                
                if (token is not IValueToken val || !val.CapableOf<ReferenceValue>(out var func))
                {
                    return rs;
                }

                return new(() =>
                {
                    if (func().HasErrored(out var error, out var refVal))
                    {
                        return error;
                    }
                    
                    if (ReferenceArgument<Gate>.TryParse(refVal).WasSuccessful(out var gate))
                    {
                        return gate;
                    }

                    return rs;
                });
            }
        );
    }
}