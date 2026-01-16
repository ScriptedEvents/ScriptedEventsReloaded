using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using MapGeneration;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.Arguments;

public class RoomsArgument(string name) : EnumHandlingArgument(name)
{
    public override string InputDescription => 
        $"{nameof(RoomName)} enum, {nameof(FacilityZone)} enum, reference to {nameof(Room)}, or * for every room";

    [UsedImplicitly]
    public DynamicTryGet<Room[]> GetConvertSolution(BaseToken token)
    {
        return ResolveEnums<Room[]>(
            token,
            new()
            {
                [typeof(RoomName)] = roomName => Room.List.Where(room => room.Name == (RoomName)roomName).ToArray(),
                [typeof(FacilityZone)] = zone => Room.List.Where(room => room.Zone == (FacilityZone)zone).ToArray(),
            },
            () =>
            {
                Result rs = $"Value '{token.RawRep}' cannot be interpreted as {InputDescription}.";
                if (token is SymbolToken { IsJoker: true })
                {
                    return Room.List.ToArray();
                }

                if (!token.CanReturn<ReferenceValue>(out var get))
                {
                    return rs;
                }

                return new(() =>
                {
                    if (get().HasErrored(out var error, out var refVal))
                    {
                        return error;
                    }
                    
                    if (ReferenceArgument<Room>.TryParse(refVal).WasSuccessful(out var room))
                    {
                        return new[] { room };
                    }

                    return rs;
                });
            }
        );
    }
}