using CustomPlayerEffects;
using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.Thirdperson.Subcontrollers;
using PlayerRoles.PlayableScps.Scp079;
using Respawning.NamingRules;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.MethodSystem.Methods.CustomRoleMethods.Structures;
using SER.Code.ValueSystem.PropertySystem;
using ValueType = SER.Code.ValueSystem.Other.ValueType;

namespace SER.Code.ValueSystem;

public class PlayerValue : Value, IValueWithProperties
{
    public PlayerValue(Player? plr)
    {
        Players = plr is not null
            ? [plr]
            : [];
    }

    public PlayerValue(IEnumerable<Player> players)
    {
        Players = players.ToArray();
    }
    
    public PlayerValue()
    {
        Players = [];
    }

    public Player[] Players { get; }

    public override bool Equals(Value? other) => other is PlayerValue otherP && Players.SequenceEqual(otherP.Players);
    
    public override int HashCode =>
        Players.Select(plr => plr.UserId).GetEnumerableHashCode().HasErrored(out var error, out var val)
            ? throw new TosoksFuckedUpException(error)
            : val;

    [UsedImplicitly]
    public new static string FriendlyName => "player value";

    public Dictionary<string, IValueWithProperties.PropInfo> Properties { get; } = 
        PropertyInfoMap.ToDictionary(pair => pair.Key.ToString().LowerFirst(), pair => pair.Value, StringComparer.OrdinalIgnoreCase)
            .Append(new KeyValuePair<string, IValueWithProperties.PropInfo>("valType", 
                new IValueWithProperties.PropInfo<PlayerValue, EnumValue<ValueType>>(_ => ValueType.Player, "The type of the value")))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);

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
        Effects,
        EffectReferences,
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
        IsNpc,
        IsDummy,
        IsSpeaking,
        IsSpectatable,
        IsJumping,
        IsGrounded,
        Stamina,
        MovementState,
        RoleColor,
        LifeId,
        UnitId,
        Unit,
        CRole
    }

    private class Info<T>(Func<Player, T> handler, string? description)
        : IValueWithProperties.PropInfo<Player, T>(handler, description) where T : Value
    {
        protected override Func<object, object>? Translator { get; } = 
            obj => obj is PlayerValue { Players.Length: 1 } val ? val.Players[0] : obj;
    }

    public static readonly Dictionary<PlayerProperty, IValueWithProperties.PropInfo> PropertyInfoMap = new()
    {
        [PlayerProperty.Name] = new Info<StaticTextValue>(plr => plr.Nickname, null),
        [PlayerProperty.DisplayName] = new Info<StaticTextValue>(plr => plr.DisplayName, null),
        [PlayerProperty.Role] = new Info<EnumValue<RoleTypeId>>(plr => plr.Role.ToEnumValue(), null),
        [PlayerProperty.RoleRef] = new Info<ReferenceValue<PlayerRoleBase>>(plr => new(plr.RoleBase), null),
        [PlayerProperty.Team] = new Info<EnumValue<Team>>(plr => plr.Team.ToEnumValue(), null),
        [PlayerProperty.Inventory] = new Info<CollectionValue<Item>>(plr => new(plr.Inventory.UserInventory.Items.Values.Select(Item.Get).RemoveNulls()), $"A collection of references to {nameof(Item)} objects"),
        [PlayerProperty.ItemCount] = new Info<NumberValue>(plr => (decimal)plr.Inventory.UserInventory.Items.Count, null),
        [PlayerProperty.HeldItemRef] = new Info<ReferenceValue<Item>>(plr => new(plr.CurrentItem), "A reference to the item the player is holding"),
        [PlayerProperty.IsAlive] = new Info<BoolValue>(plr => plr.IsAlive, null),
        [PlayerProperty.UserId] = new Info<StaticTextValue>(plr => plr.UserId, "The ID of the account (like SteamID64)"),
        [PlayerProperty.PlayerId] = new Info<NumberValue>(plr => plr.PlayerId, "The ID that the server assigned for this round"),
        [PlayerProperty.CustomInfo] = new Info<StaticTextValue>(plr => plr.CustomInfo, "Custom info set by the server"),
        [PlayerProperty.RoomRef] = new Info<ReferenceValue<Room>>(plr => new(plr.Room), "A reference to the room the player is in"),
        [PlayerProperty.Health] = new Info<NumberValue>(plr => (decimal)plr.Health, null),
        [PlayerProperty.MaxHealth] = new Info<NumberValue>(plr => (decimal)plr.MaxHealth, null),
        [PlayerProperty.ArtificialHealth] = new Info<NumberValue>(plr => (decimal)plr.ArtificialHealth, null),
        [PlayerProperty.MaxArtificialHealth] = new Info<NumberValue>(plr => (decimal)plr.MaxArtificialHealth, null),
        [PlayerProperty.HumeShield] = new Info<NumberValue>(plr => (decimal)plr.HumeShield, null),
        [PlayerProperty.MaxHumeShield] = new Info<NumberValue>(plr => (decimal)plr.MaxHumeShield, null),
        [PlayerProperty.HumeShieldRegenRate] = new Info<NumberValue>(plr => (decimal)plr.HumeShieldRegenRate, null),
        [PlayerProperty.Effects] = new Info<CollectionValue<TextValue>>(plr => new(plr.ActiveEffects.Select(e => e.GetType().Name.ToStaticTextValue())), "Collection of names of active effects"),
        [PlayerProperty.EffectReferences] = new Info<CollectionValue<ReferenceValue<StatusEffectBase>>>(plr => new(plr.ActiveEffects), "Collection of references of active effects"),
        [PlayerProperty.GroupName] = new Info<StaticTextValue>(plr => plr.GroupName, "The name of the group (like admin or vip)"),
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
        [PlayerProperty.RoleChangeReason] = new Info<EnumValue<RoleChangeReason>>(plr => plr.RoleBase.ServerSpawnReason.ToEnumValue(), null),
        [PlayerProperty.RoleSpawnFlags] = new Info<EnumValue<RoleSpawnFlags>>(plr => plr.RoleBase.ServerSpawnFlags.ToEnumValue(), null),
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
        [PlayerProperty.Emotion] = new Info<EnumValue<EmotionPresetType>>(plr => plr.Emotion.ToEnumValue(), "Current emotion (e.g. Neutral, Chad)"),
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
        [PlayerProperty.IsNpc] = new Info<BoolValue>(plr => plr.IsNpc, "True if it's a player without any client connected to it"),
        [PlayerProperty.IsDummy] = new Info<BoolValue>(plr => plr.IsDummy, null),
        [PlayerProperty.IsSpeaking] = new Info<BoolValue>(plr => plr.IsSpeaking, null),
        [PlayerProperty.IsSpectatable] = new Info<BoolValue>(plr => plr.IsSpectatable, null),
        [PlayerProperty.IsJumping] = new Info<BoolValue>(plr => plr.RoleBase is IFpcRole currentRole && currentRole.FpcModule.Motor.JumpController.IsJumping, null),
        [PlayerProperty.IsGrounded] = new Info<BoolValue>(plr => plr.ReferenceHub.IsGrounded(), null),
        [PlayerProperty.Stamina] = new Info<NumberValue>(plr => (decimal)plr.StaminaRemaining, "Returns the player's remaining stamina."),
        [PlayerProperty.MovementState] = new Info<TextValue>(plr => plr.RoleBase is IFpcRole currentRole ? currentRole.FpcModule.CurrentMovementState.ToString().ToStaticTextValue() : new("None"), "Returns the player's movement state or 'None' if the player is not a first-person role."),
        [PlayerProperty.RoleColor] = new Info<ColorValue>(plr => plr.RoleBase.RoleColor, "Returns the hex value of the player's role color."),
        [PlayerProperty.LifeId] = new Info<NumberValue>(plr => plr.LifeId, null),
        [PlayerProperty.UnitId] = new Info<NumberValue>(plr => (decimal)plr.UnitId, null),
        [PlayerProperty.Unit] = new Info<TextValue>(plr => NamingRulesManager.ClientFetchReceived(plr.Team, plr.UnitId).ToStaticTextValue(), "Returns the player's unit (e.g FOXTROT-03) if player is NTF or Facility Guard, otherwise returns an empty text value."),
        [PlayerProperty.CRole] = new Info<ReferenceValue<CRole>>(plr => CRole.AssignedRoles.TryGetValue(plr, out var role) ? role : null, "Returns the player's CRole if player is a CRole, otherwise returns an empty reference value.")
    };

    public override TryGet<object> ToCSharpObject(Type targetType)
    {
        if (targetType == typeof(Player))
        {
            if (Players.Length == 1) return Players[0];
            return "Target requires exactly one player, but multiple or none were provided.";
        }

        if (targetType.IsAssignableFrom(typeof(Player[]))) return TryGet<object>.Success(Players);
        if (targetType.IsAssignableFrom(typeof(List<Player>))) return TryGet<object>.Success(Players.ToList());
        if (targetType.IsAssignableFrom(typeof(IEnumerable<Player>))) return TryGet<object>.Success(Players.AsEnumerable());

        return $"Cannot convert players to {targetType.Name}";
    }
}