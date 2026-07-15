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

    private static readonly object BaseEnumsLock = new();
    
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
        lock (BaseEnumsLock)
        {
            if (!BaseEnums.Contains(type))
                BaseEnums.Add(type);
        }
    }
    
    public static IEnumerable<Type> GetNonReflectedEnums()
    {
        lock (BaseEnumsLock)
        {
            return BaseEnums.ToArray();
        }
    }

    public static IEnumerable<Type> GetAllEnums()
    {
        Type[] baseEnums;
        lock (BaseEnumsLock)
        {
            baseEnums = BaseEnums.ToArray();
        }
        
        foreach (var @enum in baseEnums)
        {
            yield return @enum;
        }

        foreach (var @enum in ReflectedEnums)
        {
            if (baseEnums.Contains(@enum)) continue;
            yield return @enum;
        }
    }
}