using LabApi.Features.Wrappers;
using MapGeneration;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;

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

        return ValueOrEnumResolver<Room[]>(token, value =>
        {
            return value is ReferenceValue reference
                ? reference.GetAs<Room>().OnSuccess<Room[]>(room => [room])
                : GenericError(token);
        }, [
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
