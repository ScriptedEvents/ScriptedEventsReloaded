using LabApi.Features.Wrappers;
using MapGeneration;
using PlayerRoles;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;
using SER.Code.ValueSystem;
using UnityEngine;
using Utils;

namespace SER.Code.MethodSystem.Methods.PlayerValueMethods;

[UsedImplicitly]
public class ParsePlayersMethod : ReturningMethod<PlayerValue>, IAdditionalDescription
{
    public override string Description => "Tries to parse the provided value to a player value.";

    public string AdditionalDescription =>
        "Tries to parse using: " +
        "nickname, display name, user group, user id, ip address, player id, team, role, room, zone," +
        $"{nameof(Room)} reference, {nameof(Item)} reference, {nameof(ReferenceHub)} reference, " +
        $"{nameof(GameObject)} reference, {nameof(Transform)} reference.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new AnyValueArgument("input")
    ];

    public override void Execute()
    {
        var value = Args.GetAnyValue("input");
        switch (value)
        {
            case PlayerValue playerValue:
            {
                ReturnValue = playerValue;
                return;
            }
            case LiteralValue literalValue:
            {
                var stringRep = literalValue.StringRep;

                if (Player.ReadyList.Where(p => 
                        p.Nickname == stringRep
                        || p.DisplayName == stringRep
                        || p.UserGroup?.Name == stringRep
                        || p.UserId == stringRep
                        || p.IpAddress == stringRep).ToArray() is { } players
                    && players.Any())
                {
                    ReturnValue = players.ToPlayerValue();
                    return;
                }
                
                var raParsing = RAUtils
                    .ProcessPlayerIdOrNamesList(new ArraySegment<string>([stringRep]), 0, out _);
                if (raParsing.Any())
                {
                    ReturnValue = Player.Get(raParsing).ToPlayerValue();
                    return;
                }
                
                if (EnumArgument<Team>.Convert(stringRep).WasSuccessful(out var team))
                {
                    ReturnValue = Player.ReadyList.Where(p => p.Team == team).ToPlayerValue();
                    return;
                }

                if (EnumArgument<RoleTypeId>.Convert(stringRep).WasSuccessful(out var role))
                {
                    ReturnValue = Player.ReadyList.Where(p => p.Role == role).ToPlayerValue();
                    return;
                }

                if (EnumArgument<RoomName>.Convert(stringRep).WasSuccessful(out var room))
                {
                    ReturnValue = Player.ReadyList.Where(p => p.Room?.Name == room).ToPlayerValue();
                    return;
                }

                if (EnumArgument<FacilityZone>.Convert(stringRep).WasSuccessful(out var zone))
                {
                    ReturnValue = Player.ReadyList.Where(p => p.Zone == zone).ToPlayerValue();
                    return;
                }
                break;
            }
            case ReferenceValue referenceValue:
            {
                switch (referenceValue.Value)
                {
                    case ReferenceHub referenceHub:
                    {
                        ReturnValue = Player.Get(referenceHub).ToPlayerValue();
                        return;
                    }
                    case GameObject gameObject:
                    {
                        if (Player.Get(gameObject) is { } player)
                        {
                            ReturnValue = player.ToPlayerValue();
                            return;
                        }
                        break;
                    }
                    case Transform transform:
                    {
                        if (Player.Get(transform.gameObject) is { } player)
                        {
                            ReturnValue = player.ToPlayerValue();
                            return;
                        }
                        break;
                    }
                    case Room room:
                    {
                        ReturnValue = Player.ReadyList.Where(p => p.Room == room).ToPlayerValue();
                        return;
                    }
                    case Item item:
                    {
                        if (item.CurrentOwner != null)
                        {
                            ReturnValue = item.CurrentOwner.ToPlayerValue();
                            return;
                        }
                        break;
                    }
                }
                break;
            }
        }

        ReturnValue = new PlayerValue();
    }
}