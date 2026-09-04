using LabApi.Features.Wrappers;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Extensions;

// ReSharper disable LoopCanBeConvertedToQuery

namespace SER.Code.Integrations.Ucr;

/// <summary>
/// Late-bound boundary for the optional UncomplicatedCustomRoles dependency.
/// Keep UCR types inside synchronous method bodies and never add UCR-typed fields.
/// </summary>
internal static class UcrBridge
{
    internal static object WrapEventValue(object value)
    {
        return value switch
        {
            SummonedCustomRole instance => WrapInstance(instance),
            ICustomRole role => WrapRole(role),
            _ => value
        };
    }

    internal static Type GetEventValueType(Type type)
    {
        if (typeof(SummonedCustomRole).IsAssignableFrom(type))
            return typeof(UCRRoleInstance);

        return typeof(ICustomRole).IsAssignableFrom(type)
            ? typeof(UCRRole)
            : type;
    }

    internal static bool IsCancellableEvent(Type type)
    {
        return type.GetProperty("IsAllowed") is
        {
            PropertyType: not null,
            CanWrite: true
        } property && property.PropertyType == typeof(bool);
    }

    internal static bool TrySetEventAllowed(object eventArgs, bool isAllowed)
    {
        var property = eventArgs.GetType().GetProperty("IsAllowed");
        if (property is not { PropertyType: not null, CanWrite: true } || property.PropertyType != typeof(bool))
            return false;

        property.SetValue(eventArgs, isAllowed);
        return true;
    }

    internal static UCRRole[] GetRoles()
    {
        List<UCRRole> roles = [];
        foreach (ICustomRole role in CustomRole.List)
            roles.Add(WrapRole(role));

        roles.Sort(static (left, right) => left.Id.CompareTo(right.Id));
        return roles.ToArray();
    }

    internal static UCRRole GetRole(int roleId)
    {
        if (!CustomRole.TryGet(roleId, out ICustomRole role))
            throw new KeyNotFoundException($"UCR role '{roleId}' is not registered.");

        return WrapRole(role);
    }

    internal static UCRRole GetRole(string roleName)
    {
        if (!CustomRole.TryGet(roleName, out ICustomRole role))
            throw new KeyNotFoundException($"UCR role '{roleName}' is not registered.");

        return WrapRole(role);
    }

    internal static UCRRole? GetRole(Player player)
    {
        return SummonedCustomRole.Get(player)?.Role is { } role ? WrapRole(role) : null;
    }

    internal static UCRRoleInstance? GetRoleInstance(Player player)
    {
        return SummonedCustomRole.Get(player) is { } instance ? WrapInstance(instance) : null;
    }

    internal static UCRRoleInstance[] GetRoleInstances(object reference)
    {
        ICustomRole role = GetRawRole(reference);
        List<UCRRoleInstance> instances = [];
        foreach (SummonedCustomRole instance in SummonedCustomRole.Get(role))
            instances.Add(WrapInstance(instance));

        return instances.ToArray();
    }

    internal static Player[] GetPlayersWithRole(int roleId)
    {
        List<Player> players = [];
        foreach (Player player in Player.ReadyList)
        {
            if (SummonedCustomRole.Get(player)?.Role.Id == roleId)
                players.Add(player);
        }

        return players.ToArray();
    }

    internal static int GetSpawnedCount(object reference)
    {
        return SummonedCustomRole.Count(GetRawRole(reference));
    }

    internal static bool IsRegistered(object reference)
    {
        return CustomRole.IsRegistered(GetRawRole(reference).Id);
    }

    internal static void SetRole(Player player, int roleId)
    {
        if (!CustomRole.IsRegistered(roleId))
            throw new KeyNotFoundException($"UCR role '{roleId}' is not registered.");

        player.SetCustomRole(roleId);
    }

    internal static bool RemoveRole(Player player, bool resetRole)
    {
        return player.TryRemoveCustomRole(resetRole);
    }

    private static UCRRole WrapRole(ICustomRole role)
    {
        return new UCRRole(role.Id, role.Name, role);
    }

    private static UCRRoleInstance WrapInstance(SummonedCustomRole instance)
    {
        return new UCRRoleInstance(instance.Id, instance.Player, WrapRole(instance.Role), instance);
    }

    private static ICustomRole GetRawRole(object reference)
    {
        if (reference is UCRRole { Object: ICustomRole role })
            return role;

        throw new ArgumentException("The reference is not a UCR role reference.");
    }
}
