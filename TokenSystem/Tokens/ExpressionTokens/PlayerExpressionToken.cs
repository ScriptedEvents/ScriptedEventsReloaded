using System;
using System.Collections.Generic;
using System.Linq;
using InventorySystem.Items;
using LabApi.Features.Wrappers;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079;
using SER.ArgumentSystem.Arguments;
using SER.Helpers.Extensions;
using SER.Helpers.ResultSystem;
using SER.TokenSystem.Tokens.VariableTokens;
using SER.ValueSystem;

namespace SER.TokenSystem.Tokens.ExpressionTokens;

public class PlayerExpressionToken : ExpressionToken
{
    private PlayerProperty _property = PlayerProperty.None;
    private PlayerVariableToken _pvarToken = null!;

    public enum PlayerProperty
    {
        None = 0,
        Name,
        DisplayName,
        Role,
        RoleRef,
        Team,
        Inventory,
        ItemCount,
        HeldItemRef,
        IsAlive,
        UserId,
        PlayerId,
        CustomInfo,
        RoomRef,
        Health,
        MaxHealth,
        ArtificialHealth,
        MaxArtificialHealth,
        HumeShield,
        MaxHumeShield,
        HumeShieldRegenRate,
        GroupName,
        PositionX,
        PositionY,
        PositionZ,
        IsDisarmed,
        IsMuted,
        IsIntercomMuted,
        IsGlobalModerator,
        IsNorthwoodStaff,
        IsBypassEnabled,
        IsGodModeEnabled,
        IsNoclipEnabled,
        Gravity,
        RoleChangeReason,
        RoleSpawnFlags,
        AuxiliaryPower,
        Emotion,
    }

    public abstract class Info
    {
        public abstract Func<Player, Value> Handler { get; }
        public abstract Type ReturnType { get; }
        public abstract string? Description { get; }
    }

    public class Info<T>(Func<Player, T> handler, string? description) : Info 
        where T : Value
    {
        public override Func<Player, Value> Handler => handler;
        public override Type ReturnType => typeof(T);
        public override string? Description => description;
    }

    public static readonly Dictionary<PlayerProperty, Info> PropertyInfoMap = new()
    {
        [PlayerProperty.Name] = new Info<TextValue>(plr => plr.Nickname, null),
        [PlayerProperty.DisplayName] = new Info<TextValue>(plr => plr.DisplayName, null),
        [PlayerProperty.Role] = new Info<TextValue>(plr => plr.Role.ToString(), $"Player role type ({nameof(RoleTypeId)} enum value)"),
        [PlayerProperty.RoleRef] = new Info<ReferenceValue>(plr => new(plr.RoleBase), $"Reference to {nameof(PlayerRoleBase)}"),
        [PlayerProperty.Team] = new Info<TextValue>(plr => plr.Team.ToString(), $"Player team ({nameof(Team)} enum value)"),
        [PlayerProperty.Inventory] = new Info<CollectionValue>(plr => new(plr.Inventory.UserInventory.Items.Values.ToArray()), $"A collection of references to {nameof(ItemBase)} objects"),
        [PlayerProperty.ItemCount] = new Info<NumberValue>(plr => (decimal)plr.Inventory.UserInventory.Items.Count, null),
        [PlayerProperty.HeldItemRef] = new Info<ReferenceValue>(plr => new(plr.CurrentItem), "A reference to the item the player is holding"),
        [PlayerProperty.IsAlive] = new Info<BoolValue>(plr => plr.IsAlive, null),
        [PlayerProperty.UserId] = new Info<TextValue>(plr => plr.UserId, "The ID of the account (like SteamID64)"),
        [PlayerProperty.PlayerId] = new Info<NumberValue>(plr => plr.PlayerId, "The ID that the server assigned for this round"),
        [PlayerProperty.CustomInfo] = new Info<TextValue>(plr => plr.CustomInfo, "Custom info set by the server"),
        [PlayerProperty.RoomRef] = new Info<ReferenceValue>(plr => new(plr.Room), "A reference to the room the player is in"),
        [PlayerProperty.Health] = new Info<NumberValue>(plr => (decimal)plr.Health, null),
        [PlayerProperty.MaxHealth] = new Info<NumberValue>(plr => (decimal)plr.MaxHealth, null),
        [PlayerProperty.ArtificialHealth] = new Info<NumberValue>(plr => (decimal)plr.ArtificialHealth, null),
        [PlayerProperty.MaxArtificialHealth] = new Info<NumberValue>(plr => (decimal)plr.MaxArtificialHealth, null),
        [PlayerProperty.HumeShield] = new Info<NumberValue>(plr => (decimal)plr.HumeShield, null),
        [PlayerProperty.MaxHumeShield] = new Info<NumberValue>(plr => (decimal)plr.MaxHumeShield, null),
        [PlayerProperty.HumeShieldRegenRate] = new Info<NumberValue>(plr => (decimal)plr.HumeShieldRegenRate, null),
        [PlayerProperty.GroupName] = new Info<TextValue>(plr => plr.GroupName, "The name of the group (like admin or vip)"),
        [PlayerProperty.PositionX] = new Info<NumberValue>(plr => (decimal)plr.Position.x, null),
        [PlayerProperty.PositionY] = new Info<NumberValue>(plr => (decimal)plr.Position.y, null),
        [PlayerProperty.PositionZ] = new Info<NumberValue>(plr => (decimal)plr.Position.z, null),
        [PlayerProperty.IsDisarmed] = new Info<BoolValue>(plr => plr.IsDisarmed, null),
        [PlayerProperty.IsMuted] = new Info<BoolValue>(plr => plr.IsMuted, null),
        [PlayerProperty.IsIntercomMuted] = new Info<BoolValue>(plr => plr.IsIntercomMuted, null),
        [PlayerProperty.IsGlobalModerator] = new Info<BoolValue>(plr => plr.IsGlobalModerator, null),
        [PlayerProperty.IsNorthwoodStaff] = new Info<BoolValue>(plr => plr.IsNorthwoodStaff, null),
        [PlayerProperty.IsBypassEnabled] = new Info<BoolValue>(plr => plr.IsBypassEnabled, null),
        [PlayerProperty.IsGodModeEnabled] = new Info<BoolValue>(plr => plr.IsGodModeEnabled, null),
        [PlayerProperty.IsNoclipEnabled] = new Info<BoolValue>(plr => plr.IsNoclipEnabled, null),
        [PlayerProperty.Gravity] = new Info<NumberValue>(plr => -(decimal)plr.Gravity.y, null),
        [PlayerProperty.RoleChangeReason] = new Info<TextValue>(plr => plr.RoleBase._spawnReason.ToString(), null),
        [PlayerProperty.RoleSpawnFlags] = new Info<TextValue>(plr => plr.RoleBase._spawnFlags.ToString(), null),
        [PlayerProperty.AuxiliaryPower] = new Info<NumberValue>(plr =>
        {
            if (plr.RoleBase is Scp079Role scp)
            {
                if (scp.SubroutineModule.TryGetSubroutine<Scp079TierManager>(out Scp079TierManager tier))
                {
                    return tier.TotalExp;
                }
                else return -1;
            }
            else return -1;
        }, "Returns player EXP if he is SCP-079, otherwise returns -1"),
        [PlayerProperty.Emotion] = new Info<TextValue>(plr => plr.Emotion.ToString(), "Current emotion (e.g. Neutral, Chad)"),
    };

    protected override IParseResult InternalParse(BaseToken[] tokens)
    {
        if (tokens.First() is not PlayerVariableToken pvarToken)
        {
            return new Ignore();
        }
        
        _pvarToken = pvarToken;

        switch (tokens.Length)
        {
            case < 2:
                return new Error("A player expression expects to have an argument, but none was provided.");
            case > 2:
                return new Error(
                    $"A player expression expects to have only one argument, but {tokens.Length - 1} were provided.");
        }

        if (EnumArgument<PlayerProperty>.Convert(tokens.Last(), Script)
            .HasErrored(out var error, out var property))
        {
            return new Error(error);
        }

        _property = property;
        return new Success();
    }

    public override TryGet<Value> Value()
    {
        if (_pvarToken.TryGetVariable().HasErrored(out var err, out var variable))
        {
            return err;
        }
        
        return variable.Players.Len switch
        {
            < 1 => $"Player variable '{variable.Name}' has no players.",
            > 1 => $"Player variable '{variable.Name}' has more than one player.",
            _ => PropertyInfoMap[_property].Handler(variable.Players.First())
        };
    }

    public override Type[]? PossibleValueTypes => null;
}
