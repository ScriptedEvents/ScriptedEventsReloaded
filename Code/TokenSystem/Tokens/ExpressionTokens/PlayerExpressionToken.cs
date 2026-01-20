using LabApi.Features.Wrappers;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;

namespace SER.Code.TokenSystem.Tokens.ExpressionTokens;

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
        Experience,
        MaxAuxiliaryPower,
        SizeX,
        SizeY,
        SizeZ,
        AccessTier,
        RelativeX,
        RelativeY,
        RelativeZ,
    }

    public abstract class Info
    {
        public abstract Func<Player, Value> Handler { get; }
        public abstract SingleTypeOfValue ReturnType { get; }
        public abstract string? Description { get; }
    }

    public class Info<T>(Func<Player, T> handler, string? description) : Info 
        where T : Value
    {
        public override Func<Player, Value> Handler => handler;
        public override SingleTypeOfValue ReturnType => new(typeof(T));
        public override string? Description => description;
    }

    public static readonly Dictionary<PlayerProperty, Info> PropertyInfoMap = new()
    {
        [PlayerProperty.Name] = new Info<TextValue>(plr => plr.Nickname, null),
        [PlayerProperty.DisplayName] = new Info<TextValue>(plr => plr.DisplayName, null),
        [PlayerProperty.Role] = new Info<TextValue>(plr => plr.Role.ToString(), $"Player role type ({nameof(RoleTypeId)} enum value)"),
        [PlayerProperty.RoleRef] = new Info<ReferenceValue>(plr => new(plr.RoleBase), $"Reference to {nameof(PlayerRoleBase)}"),
        [PlayerProperty.Team] = new Info<TextValue>(plr => plr.Team.ToString(), $"Player team ({nameof(Team)} enum value)"),
        [PlayerProperty.Inventory] = new Info<CollectionValue>(plr => new(plr.Inventory.UserInventory.Items.Values.Select(Item.Get).RemoveNulls()), $"A collection of references to {nameof(Item)} objects"),
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
        [PlayerProperty.RoleChangeReason] = new Info<TextValue>(plr => plr.RoleBase.ServerSpawnReason.ToString(), null),
        [PlayerProperty.RoleSpawnFlags] = new Info<TextValue>(plr => plr.RoleBase.ServerSpawnFlags.ToString(), null),
        [PlayerProperty.AuxiliaryPower] = new Info<NumberValue>(plr =>
        {
            if (plr.RoleBase is Scp079Role scp)
            {
                if (scp.SubroutineModule.TryGetSubroutine(out Scp079AuxManager man))
                {
                    return (decimal)man.CurrentAux;
                }
            }

            return -1;
        }, "Returns player Aux power if he is SCP-079, otherwise returns -1"),
        [PlayerProperty.Experience] = new Info<NumberValue>(plr =>
        {
            if (plr.RoleBase is Scp079Role scp)
            {
                if (scp.SubroutineModule.TryGetSubroutine(out Scp079TierManager tier))
                {
                    return tier.TotalExp;
                }
            }

            return -1;
        }, "Returns player EXP if he is SCP-079, otherwise returns -1"),
        [PlayerProperty.Emotion] = new Info<TextValue>(plr => plr.Emotion.ToString(), "Current emotion (e.g. Neutral, Chad)"),
        [PlayerProperty.MaxAuxiliaryPower] = new Info<NumberValue>(plr =>
        {
            if (plr.RoleBase is Scp079Role scp)
            {
                if (scp.SubroutineModule.TryGetSubroutine(out Scp079AuxManager man))
                {
                    return (decimal)man.MaxAux;
                }
            }

            return -1;
        }, "Returns the player's Maximum Auxiliary Power if they are SCP-079, otherwise returns -1"),
        [PlayerProperty.SizeX] = new Info<NumberValue>(plr => (decimal)plr.Scale.x, null),
        [PlayerProperty.SizeY] = new Info<NumberValue>(plr => (decimal)plr.Scale.y, null),
        [PlayerProperty.SizeZ] = new Info<NumberValue>(plr => (decimal)plr.Scale.z, null),
        [PlayerProperty.AccessTier] = new Info<NumberValue>(plr =>
        {
            if (plr.RoleBase is Scp079Role scp)
            {
                if (scp.SubroutineModule.TryGetSubroutine(out Scp079TierManager tier))
                {
                    return tier.AccessTierLevel;
                }
            }

            return -1;
        }, "Returns the player's Access Tier Level if they are SCP-079, otherwise returns -1"),
        [PlayerProperty.RelativeX] = new Info<NumberValue>(plr => (decimal)plr.RelativeRoomPosition().x, "Returns the player's x relative to the current room or 0 if in no room"),
        [PlayerProperty.RelativeY] = new Info<NumberValue>(plr => (decimal)plr.RelativeRoomPosition().y, "Returns the player's y relative to the current room or 0 if in no room"),
        [PlayerProperty.RelativeZ] = new Info<NumberValue>(plr => (decimal)plr.RelativeRoomPosition().z, "Returns the player's z relative to the current room or 0 if in no room"),
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

    public override TypeOfValue PossibleValues => new UnknownTypeOfValue();
}
