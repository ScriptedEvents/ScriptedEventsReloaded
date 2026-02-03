using LabApi.Features.Wrappers;
using PlayerRoles.PlayableScps.Scp096;
using PlayerRoles.PlayableScps.Scp173;
using SER.Code.Exceptions;
using UnityEngine;

namespace SER.Code.Extensions;

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
                ? throw new NotOurFaultException("Guess what? THAT PEANUT DOESN'T HAVE EVERY CORE MODULE " +
                                                 "(NW's fault obviously)")
                : observersTracker;

        public Player[] ObservingPlayers => peanut.ObserversTracker.Observers
            .Select(Player.Get)
            .RemoveNulls()
            .ToArray();
    }

    extension(Scp096Role shyGuy)
    {
        public Scp096TargetsTracker TargetsTracker =>
            !shyGuy.SubroutineModule.TryGetSubroutine(out Scp096TargetsTracker targetsTracker)
                ? throw new NotOurFaultException("Apparently the 096 is missing some CORE features " +
                                                 "(NW's fault obviously)")
                : targetsTracker;
        
        public Player[] Targets => shyGuy.TargetsTracker.Targets
            .Select(Player.Get)
            .RemoveNulls()
            .ToArray(); 
    }
}