using LabApi.Features.Wrappers;
using UnityEngine;

namespace SER.Helpers.Extensions;

public static class PlayerExtensions
{
    public static Vector3 RelativeRoomPosition(this Player player)
    {
        return player.Room == null ? new(0,0,0) : player.Room.Transform.InverseTransformPoint(player.Position) - new Vector3(0, player.Scale.y + 0.01f, 0);
    }
}