using LabApi.Features.Enums;
using LabApi.Features.Wrappers;
using MapGeneration;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ArgumentSystem.Arguments;

public class DoorArgument(string name) : EnumHandlingArgument(name)
{
    public override string InputDescription => 
        $"{nameof(DoorName)} enum, " +
        $"{nameof(RoomName)} enum, " +
        $"{nameof(FacilityZone)} enum " +
        $"or a {nameof(Door)} reference.";

    [UsedImplicitly]
    public OldDynamicTryGet<Door> GetConvertSolution(BaseToken token)
    {
        if (token.CanReturnReference<Door>(out var func))
        {
            return func;
        }
        
        return EnumResolver<Door>(token, [
            new EnumHandler<DoorName, Door>(name => new(() =>
            {
                return Door.List
                    .Where(door => door.DoorName == name)
                    .TryGetRandomValue($"Door with name '{name}' does not exist.");
            })),
            new EnumHandler<FacilityZone, Door>(zone => new(() =>
            {
                return Door.List.Where(d => d.Zone == zone)
                    .Where(d => d is not ElevatorDoor)
                    .TryGetRandomValue($"No doors in zone '{zone}' exist.");
            })),
            new EnumHandler<RoomName, Door>(name => new(() =>
            {
                return Room.List.Where(r => r.Name == name)
                    .SelectMany(r => r.Doors)
                    .Where(d => d is not ElevatorDoor)
                    .TryGetRandomValue($"No doors in room '{name}' exist.");
            }))
        ]);
    }
}