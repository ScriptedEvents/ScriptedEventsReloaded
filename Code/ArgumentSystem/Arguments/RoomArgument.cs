using LabApi.Features.Wrappers;
using MapGeneration;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ArgumentSystem.Arguments;

public class RoomArgument(string name) : EnumHandlingArgument(name)
{
    public override string InputDescription => 
        $"{nameof(RoomName)} enum, " +
        $"{nameof(FacilityZone)} enum " +
        $"or reference to {nameof(Room)}";

    [UsedImplicitly]
    public DynamicTryGet<Room> GetConvertSolution(BaseToken token)
    {
        if (token.CanReturnReference<Room>(out var func))
        {
            return func;
        }

        return EnumResolver<Room>(token, [
            new EnumHandler<RoomName, Room>(roomName => new(() =>
            {
                return Room.List
                    .Where(room => room.Name == roomName)
                    .TryGetRandomValue($"Room with name '{roomName}' does not exist.");
            })),
            new EnumHandler<FacilityZone, Room>(zone => new(() =>
            {
                return Room.List
                    .Where(room => room.Zone == zone)
                    .TryGetRandomValue($"No rooms in zone '{zone}' exist.");
            }))
        ]);
    }
}