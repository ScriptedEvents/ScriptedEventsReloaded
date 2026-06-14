using LabApi.Features.Enums;
using LabApi.Features.Wrappers;
using MapGeneration;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ArgumentSystem.Arguments;

public class DoorsArgument(string name) : EnumHandlingArgument(name)
{
    public override string InputDescription =>
        $"{nameof(DoorName)} enum, " +
        $"{nameof(FacilityZone)} enum, " +
        $"{nameof(RoomName)} enum, " +
        $"reference to {nameof(Door)}, " +
        $"reference to {nameof(Room)} " +
        $"or 'all' for every door";

    [UsedImplicitly]
    public DynamicTryGet<Door[]> GetConvertSolution(BaseToken token)
    {
        if (token is SymbolToken { IsJoker: true } or AllToken)
        {
            return new(() => Door.List.Where(d => d is not ElevatorDoor).ToArray());
        }
        
        if (token.CanReturn<ReferenceValue>(out var get))
        {
            return new(() =>
            {
                if (get().HasErrored(out var error, out var refToken))
                {
                    return error;
                }

                if (refToken.ValueIs<Door>(out var door))
                {
                    return new[] { door };
                }

                if (refToken.ValueIs<Room>(out var room))
                {
                    return room.Doors.ToArray();
                }

                return GenericError(token);
            });
        }

        return EnumResolver<Door[]>(token, [
            new EnumHandler<DoorName, Door[]>(name => new(() =>
            {
                return Door.List
                .Where(door => door.DoorName == name)
                .ToArray();
            })),
            new EnumHandler<FacilityZone, Door[]>(zone => new(() =>
            {
                return Door.List
                    .Where(door => door.Zone == zone)
                    .ToArray();
            })),
            new EnumHandler<RoomName, Door[]>(name => new(() =>
            {
                return Door.List
                    .Where(d => 
                        d.Rooms.Any(r => r.Name == name) 
                        && d is not ElevatorDoor)
                    .Distinct()
                    .ToArray();
            }))
        ]);
    }
}