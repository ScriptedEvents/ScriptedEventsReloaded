using System.Reflection;
using Interactables.Interobjects;
using LabApi.Features.Enums;
using MapGeneration;
using PlayerRoles;
using SER.Code.FlagSystem.Flags;
using SER.Code.Helpers;
using ValueType = SER.Code.ValueSystem.Other.ValueType;

namespace SER.Code.Plugin.Commands.HelpSystem;

public static class EnumIndex
{
    private static readonly List<Type> BaseEnums = [
        typeof(RoomName),
        typeof(FacilityZone),
        typeof(DoorName),
        typeof(ItemType),
        typeof(ElevatorGroup),
        typeof(CustomCommandFlag.ConsoleType),
        typeof(ValueType),
        typeof(Team)
    ];
    
    private static Type[]? _reflectedEnums;
    private static Type[] ReflectedEnums => _reflectedEnums ??=
        AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(assembly =>
            {
                if (!ReflectionHelper.ShouldBeConsidered(assembly))
                {
                    return [];
                }
                
                try
                {
                    return assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException re)
                {
                    return re.Types.Where(t => t != null);
                }
                catch
                {
                    return [];
                }
            })
            .Where(t => t.IsEnum)
            .ToArray();

    public static void AddEnum(Type type)
    {
        if (!BaseEnums.Contains(type))
            BaseEnums.Add(type);
    }
    
    public static IEnumerable<Type> GetNonReflectedEnums()
    {
        return BaseEnums;
    }

    public static IEnumerable<Type> GetAllEnums()
    {
        foreach (var @enum in BaseEnums)
        {
            yield return @enum;
        }

        foreach (var @enum in ReflectedEnums)
        {
            if (BaseEnums.Contains(@enum)) continue;
            yield return @enum;
        }
    }
}