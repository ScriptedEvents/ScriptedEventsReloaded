using LabApi.Features.Enums;
using LabApi.Features.Wrappers;
using MapGeneration;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
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
    public DynamicTryGet<Door> GetConvertSolution(BaseToken token)
    {
        if (token.CanReturnReference<Door>(out var func))
        {
            return func;
        }
        
        return EnumResolver<Door>(token, [
            new EnumHandler<DoorName, Door>(name =>
            {
                var door = Door.List
                    .Where(door => door.DoorName == name)
                    .GetRandomValue();
                if (door is null)
                {
                    return $"Door with name '{name}' does not exist.";
                }
                
                return door;
            }),
            new EnumHandler<FacilityZone, Door>(zone =>
            {
                var door = Door.List.Where(d => d.Zone == zone)
                    .Where(d => d is not ElevatorDoor)
                    .GetRandomValue();
                if (door is null)
                {
                    return $"No doors in zone '{zone}' exist.";
                }
                
                return door;
            }),
            new EnumHandler<RoomName, Door>(name =>
            {
                var door = Room.List.Where(r => r.Name == name)
                    .SelectMany(r => r.Doors)
                    .Where(d => d is not ElevatorDoor)
                    .GetRandomValue();
                if (door is null)
                {
                    return $"No doors in zone '{name}' exist.";
                }
                
                return door;
            })
        ]);
    }
}