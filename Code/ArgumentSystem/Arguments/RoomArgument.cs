using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using MapGeneration;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.Arguments;

public class RoomArgument(string name) : EnumHandlingArgument(name)
{
    public override string InputDescription => $"{nameof(RoomName)} enum or reference to {nameof(Room)}";

    [UsedImplicitly]
    public DynamicTryGet<Room> GetConvertSolution(BaseToken token)
    {
        return ResolveEnums<Room>(
            token,
            new()
            {
                [typeof(RoomName)] = roomName => Room.List.First(room => room.Name == (RoomName)roomName),
            },
            () =>
            {
                Result rs = $"Value '{token.RawRep}' cannot be interpreted as {InputDescription}.";
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
                        return room;
                    }

                    return rs;
                });
            }
        );
    }
}