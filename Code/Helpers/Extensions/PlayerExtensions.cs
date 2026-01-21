using LabApi.Features.Wrappers;
using PlayerRoles.PlayableScps.Scp173;
using UnityEngine;

namespace SER.Code.Helpers.Extensions;

public static class PlayerExtensions
{
    public static Vector3 RelativeRoomPosition(this Player player)
    {
        return player.Room == null ? new(0,0,0) : player.Room.Transform.InverseTransformPoint(player.Position) - new Vector3(0, player.Scale.y + 0.01f, 0);
    }

    extension(Scp173Role peanut)
    {
        public Scp173ObserversTracker ObserversTracker =>
            !peanut.SubroutineModule.TryGetSubroutine(out Scp173ObserversTracker observersTracker)
                ? throw new Exception("I fucking hate Northwood so much.")
                : observersTracker;

        public Player[] ObservingPlayers => peanut.ObserversTracker.Observers
            .Select(Player.Get)
            .RemoveNulls()
            .ToArray();
    }
}