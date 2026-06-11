using LabApi.Features.Wrappers;
using MapGeneration;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ArgumentSystem.Arguments;

public class RoomsArgument(string name) : EnumHandlingArgument(name)
{
    public override string InputDescription =>
        $"{nameof(RoomName)} enum, " +
        $"{nameof(FacilityZone)} enum, " +
        $"reference to {nameof(Room)}, " +
        $"or 'all' for every room";

    [UsedImplicitly]
    public DynamicTryGet<Room[]> GetConvertSolution(BaseToken token)
    {
        if (token is SymbolToken { IsJoker: true } or AllToken)
        {
            return new(() => Room.List.ToArray());
        }

        if (token.CanReturnReference<Room>(out var func))
        {
            return new(() => func().OnSuccess<Room[]>(room => [room]));
        }

        return EnumResolver<Room[]>(token, [
            new EnumHandler<RoomName, Room[]>(roomName => new(() =>
            {
                return Room.List
                    .Where(room => room.Name == roomName)
                    .ToArray();
            })),
            new EnumHandler<FacilityZone, Room[]>(zone => new(() =>
            {
                return Room.List
                    .Where(room => room.Zone == zone)
                    .ToArray();
            }))
        ]);
    }
}