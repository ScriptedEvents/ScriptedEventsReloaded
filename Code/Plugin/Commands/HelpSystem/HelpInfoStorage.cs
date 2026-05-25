using Interactables.Interobjects;
using LabApi.Features.Enums;
using MapGeneration;
using SER.Code.FlagSystem.Flags;

namespace SER.Code.Plugin.Commands.HelpSystem;

public static class HelpInfoStorage
{
    public static readonly HashSet<Type> UsedEnums =
    [
        typeof(RoomName),
        typeof(FacilityZone),
        typeof(DoorName),
        typeof(ItemType),
        typeof(ElevatorGroup),
        typeof(CustomCommandFlag.ConsoleType)
    ];
}