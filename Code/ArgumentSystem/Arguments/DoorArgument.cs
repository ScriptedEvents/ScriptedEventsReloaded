using JetBrains.Annotations;
using LabApi.Features.Enums;
using LabApi.Features.Wrappers;
using MapGeneration;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.Arguments;

public class DoorArgument(string name) : EnumHandlingArgument(name)
{
    public override string InputDescription => $"{nameof(DoorName)} enum, {nameof(FacilityZone)} enum or reference to {nameof(Door)}";

    [UsedImplicitly]
    public DynamicTryGet<Door> GetConvertSolution(BaseToken token)
    {
        return ResolveEnums<Door>(
            token,
            new()
            {
                [typeof(DoorName)] = doorName =>
                {
                    var door = Door.List.Where(door => door.DoorName == (DoorName)doorName).GetRandomValue();
                    if (door is null)
                    {
                        return $"Door with name '{doorName}' does not exist.";
                    }   

                    return door;
                },
                [typeof(FacilityZone)] = zone =>
                {
                    var door = Door.List.Where(d => d.Zone == (FacilityZone)zone)
                        .Where(d => d is not ElevatorDoor)
                        .GetRandomValue();
                    if (door is null)
                    {
                        return $"No doors in zone '{zone}' exist.";
                    }

                    return door;
                }
            },
            () =>
            {
                Result rs = $"Value '{token.RawRep}' cannot be interpreted as {InputDescription}.";
                
                if (token is not IValueToken val || !val.CanReturn<ReferenceValue>(out var func))
                {
                    return rs;
                }

                return new(() =>
                {
                    if (func().HasErrored(out var error, out var refVal))
                    {
                        return error;
                    }
                    
                    if (ReferenceArgument<Door>.TryParse(refVal).WasSuccessful(out var door))
                    {
                        return door;
                    }

                    return rs;
                });
            }
        );
    }
}