using System.Reflection;
using InventorySystem.Items.Firearms;
using LabApi.Features.Wrappers;
using Newtonsoft.Json.Linq;
using PlayerStatsSystem;
using Respawning;
using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.MethodSystem.Methods.PlayerMethods;
using SER.Code.ValueSystem;
using ValueType = SER.Code.ValueSystem.ValueType;

namespace SER.Code.PropertySystem;

public static class ReferencePropertyRegistry
{
    private static readonly Dictionary<Type, Dictionary<string, IValueWithProperties.PropInfo>> RegisteredProperties = new();
    private static readonly Dictionary<Type, Dictionary<string, IValueWithProperties.PropInfo>> CachedCombinedProperties = new();

    public static IEnumerable<Type> GetRegisteredTypes() => RegisteredProperties.Keys;

    public static void Register<T, TValue>(string name, Func<T, TValue> handler, string? description = null) where TValue : Value
    {
        var type = typeof(T);
        if (!RegisteredProperties.TryGetValue(type, out var props))
        {
            props = new Dictionary<string, IValueWithProperties.PropInfo>(StringComparer.OrdinalIgnoreCase);
            RegisteredProperties[type] = props;
        }
        props[name] = new ReferencePropInfo<T, TValue>(handler, description);
        CachedCombinedProperties.Clear(); // Invalidate cache
    }

    public static Dictionary<string, IValueWithProperties.PropInfo> GetProperties(Type type)
    {
        if (CachedCombinedProperties.TryGetValue(type, out var cached))
            return cached;

        var combined = new Dictionary<string, IValueWithProperties.PropInfo>(StringComparer.OrdinalIgnoreCase);
        
        var currentType = type;
        while (currentType != null)
        {
            if (RegisteredProperties.TryGetValue(currentType, out var registered))
            {
                foreach (var kvp in registered)
                {
                    if (!combined.ContainsKey(kvp.Key))
                        combined[kvp.Key] = kvp.Value;
                }
            }
            currentType = currentType.BaseType;
        }
        
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var key = prop.Name.LowerFirst();
            if (!combined.ContainsKey(key))
            {
                combined[key] = new UnsafeReferencePropInfo(type, prop, XmlDocReader.GetDescription(prop));
            }
        }

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            var key = field.Name.LowerFirst();
            if (!combined.ContainsKey(key))
            {
                combined[key] = new UnsafeReferencePropInfo(type, field, XmlDocReader.GetDescription(field));
            }
        }
        
        combined.Add(
            "isValid", 
            new IValueWithProperties.PropInfo<ReferenceValue, BoolValue>(
                rv => rv.IsValid, 
                "Whether the reference is valid"
            )
        );
        
        combined.Add(
            "isInvalid", 
            new IValueWithProperties.PropInfo<ReferenceValue, BoolValue>(
                rv => !rv.IsValid, 
                "Whether the reference is invalid"
            )
        );
        
        combined.Add(
            "refName", 
            new IValueWithProperties.PropInfo<ReferenceValue, StaticTextValue>(
                rv => rv.Value.GetType().AccurateName, 
                "Returns the name of the held object"
            )
        );
        
        combined.Add(
            "refAssembly", 
            new IValueWithProperties.PropInfo<ReferenceValue, StaticTextValue>(
                rv => rv.Value.GetType().Assembly.GetName().Name, 
                "Returns the name of the assembly the object is defined in"
            )
        );
        
        combined.Add(
            "valType", 
            new IValueWithProperties.PropInfo<ReferenceValue, EnumValue<ValueType>>(
                _ => ValueType.Reference, 
                "The type of the value"
            )
        );

        CachedCombinedProperties[type] = combined;
        return type == typeof(JObject) || type == typeof(JToken) || type.IsSubclassOf(typeof(JToken)) 
            ? new JTokenPropertyDictionary(combined) 
            : combined;
    }

    private class JTokenPropertyDictionary(Dictionary<string, IValueWithProperties.PropInfo> inner) 
        : Dictionary<string, IValueWithProperties.PropInfo>(inner, StringComparer.OrdinalIgnoreCase), IValueWithProperties.IDynamicPropertyDictionary
    {
        public new bool TryGetValue(string key, out IValueWithProperties.PropInfo value)
        {
            if (base.TryGetValue(key, out value)) return true;
            
            // For JObject/JToken, if it's not a registered property, it's a dynamic access to a JSON property
            value = new JTokenDynamicPropInfo(key);
            return true;
        }
    }

    private class JTokenDynamicPropInfo(string key) : IValueWithProperties.PropInfo
    {
        public override OldTryGet<Value> GetValue(object obj)
        {
            JToken? token = obj switch
            {
                ReferenceValue refVal => refVal.Value as JToken,
                JToken t => t,
                _ => null
            };

            if (token == null) return "Value is not a JSON token";
            if (token is not JObject jobj) return "Value is not a JSON object, cannot access properties";

            if (!jobj.TryGetValue(key, out var val)) return $"Property '{key}' not found in JSON object";

            return Value.Parse(val);
        }

        public override SingleTypeOfValue ReturnType => new(typeof(ReferenceValue));
        public override string Description => $"Accesses JSON property '{key}'";
    }

    private class ReferencePropInfo<T, TValue>(Func<T, TValue> handler, string? description) 
        : IValueWithProperties.PropInfo<T, TValue>(handler, description) where TValue : Value
    {
        protected override Func<object, object> Translator => 
            obj => obj switch {
                ReferenceValue refVal => refVal.Value,
                PlayerValue { Players.Length: 1 } plrVal => plrVal.Players[0],
                IValueWithProperties valWithProps and T => valWithProps,
                _ => obj
            };
    }

    private static bool _isInitialized;
    public static void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        
        Register<Item, BoolValue>("inInventory", i => i.CurrentOwner is not null && i.CurrentOwner.Items.Contains(i), "Whether the item is in a player inventory");
        
        Register<Door, NumberValue>("remainingHealth", d => new NumberValue(d is BreakableDoor bDoor ? (decimal)bDoor.Health : -1), "The remaining health of the door");
        Register<Door, NumberValue>("maxHealth", d => new NumberValue(d is BreakableDoor bDoor ? (decimal)bDoor.MaxHealth : -1), "The maximum health of the door");
        Register<Door, BoolValue>("isGate", d => d is Gate, "Is the door a gate?");
        Register<Door, BoolValue>("isBreakable", d => d is BreakableDoor, "Is the door breakable?");
        Register<Door, BoolValue>("isCheckpoint", d => d is CheckpointDoor, "Is the door a part of a checkpoint?");
        
        Register<Pickup, NumberValue>("posX", p => new NumberValue((decimal)p.Position.x), "The X position of the pickup");
        Register<Pickup, NumberValue>("posY", p => new NumberValue((decimal)p.Position.y), "The Y position of the pickup");
        Register<Pickup, NumberValue>("posZ", p => new NumberValue((decimal)p.Position.z), "The Z position of the pickup");
        
        Register<Room, NumberValue>("posX", r => new NumberValue((decimal)r.Position.x), "The X position of the room");
        Register<Room, NumberValue>("posY", r => new NumberValue((decimal)r.Position.y), "The Y position of the room");
        Register<Room, NumberValue>("posZ", r => new NumberValue((decimal)r.Position.z), "The Z position of the room");
        
        Register<DamageHandlerBase, NumberValue>("damage", h => new NumberValue((decimal)((h as StandardDamageHandler)?.Damage ?? -1)), "Damage amount, -1 if not applicable");
        Register<DamageHandlerBase, EnumValue<HitboxType>>("hitbox", h => (h as StandardDamageHandler)?.Hitbox.ToEnumValue() ?? new EnumValue<HitboxType>(), "Hitbox type");
        Register<DamageHandlerBase, ReferenceValue<Firearm>>("firearmUsed", h => (h as FirearmDamageHandler)?.Firearm, "Firearm used");
        Register<DamageHandlerBase, PlayerValue>("attacker", h => new PlayerValue(Player.Get((h as AttackerDamageHandler)?.Attacker.PlayerId ?? -1)), "Attacker player");
        
        Register<RespawnWave, NumberValue>("respawnTokens", w => new NumberValue(w.Base is Respawning.Waves.Generic.ILimitedWave limitedWave ? limitedWave.RespawnTokens : -1), "Respawn tokens");
        Register<RespawnWave, NumberValue>("influence", w => new NumberValue((decimal)FactionInfluenceManager.Get(w.Faction)), "Faction influence");
        Register<RespawnWave, DurationValue>("timeLeft", w => new DurationValue(TimeSpan.FromSeconds(w.TimeLeft)), "Time left for wave");

        Register<JObject, Value>("value", obj => Value.Parse(obj), "The value of the JSON object");
        
        Register<JToken, EnumValue<JTokenType>>("type", t => t.Type.ToEnumValue(), "The type of the JSON token");
        Register<JToken, StaticTextValue>("path", t => new StaticTextValue(t.Path), "The path of the JSON token");
        Register<JToken, ReferenceValue<JToken>>("root", t => t.Root, "The root of the JSON token");
        Register<JToken, ReferenceValue<JToken>>("parent", t => t.Parent, "The parent of the JSON token");
        Register<JToken, CollectionValue<ReferenceValue<JToken>>>("children", t => new CollectionValue<ReferenceValue<JToken>>(t.Children()), "The children of the JSON token");
        Register<JToken, StaticTextValue>("asString", t => new StaticTextValue(t.ToString()), "The JSON representation of the token");
        Register<JToken, NumberValue>("asNumber", t => new NumberValue(t.Type is JTokenType.Integer or JTokenType.Float ? (decimal)t : 0), "The numeric value of the token");
        Register<JToken, BoolValue>("asBool", t => new BoolValue(t.Type == JTokenType.Boolean && (bool)t), "The boolean value of the token");
        
        Register<IPInfo, BoolValue>("isVPN", v => v.IsVPN, "Whether the IP is a VPN.");
        Register<IPInfo, BoolValue>("isHosting", v => v.IsHosting, "Whether the IP is a hosting/datacenter service.");
        Register<IPInfo, StaticTextValue>("provider", v => v.Provider, "The ISP or ASN provider name.");
        Register<IPInfo, StaticTextValue>("country", v => v.Country, "The country of the IP address.");
        Register<IPInfo, StaticTextValue>("type", v => v.Type, "The type of connection.");
        Register<IPInfo, NumberValue>("riskScore", v => new NumberValue(v.RiskScore), "The risk score of the IP address (0-100).");
        Register<IPInfo, NumberValue>("confidence", v => new NumberValue(v.Confidence), "The confidence score of the detection (0-100).");
        Register<IPInfo, StaticTextValue>("firstSeen", v => v.FirstSeen, "When the IP was first seen in detections.");
        Register<IPInfo, StaticTextValue>("lastSeen", v => v.LastSeen, "When the IP was last seen in detections.");

        foreach (var (key, propInfo) in PlayerValue.PropertyInfoMap)
        {
            var name = key.ToString().LowerFirst();
            if (!RegisteredProperties.TryGetValue(typeof(Player), out var playerProps))
            {
                playerProps = new Dictionary<string, IValueWithProperties.PropInfo>(StringComparer.OrdinalIgnoreCase);
                RegisteredProperties[typeof(Player)] = playerProps;
            }
            playerProps[name] = propInfo;
        }
    }


    private class UnsafeReferencePropInfo : IValueWithProperties.PropInfo
    {
        private readonly Type _ownerType;
        private readonly MemberInfo _member;
        private readonly Type _guessedValueType;
        private readonly Type _memberType;
        private readonly string? _description;

        public UnsafeReferencePropInfo(Type ownerType, MemberInfo member, string? description)
        {
            _ownerType = ownerType;
            _member = member;
            _description = description;

            _memberType = member switch
            {
                PropertyInfo prop => prop.PropertyType,
                FieldInfo field => field.FieldType,
                _ => typeof(object)
            };
            _guessedValueType = Value.GuessValueMetadata(_memberType);
            IsSettable = member switch
            {
                PropertyInfo prop => prop.CanWrite,
                FieldInfo field => field is { IsInitOnly: false, IsLiteral: false },
                _ => false
            };
        }

        public override OldTryGet<Value> GetValue(object obj)
        {
            object? target = obj switch
            {
                ReferenceValue refVal => refVal.Value,
                PlayerValue { Players.Length: 1 } plrVal => plrVal.Players[0],
                _ => obj
            };

            if (target == null) return "Reference is null";
            if (!_ownerType.IsInstanceOfType(target)) 
                return $"Object is not of type {_ownerType.AccurateName}";

            try
            {
                object? result = _member switch
                {
                    PropertyInfo prop => prop.GetValue(target),
                    FieldInfo field => field.GetValue(target),
                    _ => throw new InvalidOperationException()
                };

                if (result == null) return "Value is null";
                return Value.Parse(result);
            }
            catch (Exception e)
            {
                return $"Failed to get unsafe property {_member.Name}: {e.Message}";
            }
        }

        public override OldResult SetValue(object obj, Value value)
        {
            if (!IsSettable) return "Property is read-only.";

            object? target = obj switch
            {
                ReferenceValue refVal => refVal.Value,
                PlayerValue { Players.Length: 1 } plrVal => plrVal.Players[0],
                _ => obj
            };

            if (target == null) return "Reference is null";
            if (!_ownerType.IsInstanceOfType(target)) 
                return $"Object is not of type {_ownerType.AccurateName}";

            var conversionResult = value.ToCSharpObject(_memberType);
            if (conversionResult.HasErrored(out var error, out var convertedValue))
                return error;

            try
            {
                if (_member is PropertyInfo prop) prop.SetValue(target, convertedValue);
                else if (_member is FieldInfo field) field.SetValue(target, convertedValue);
                return true;
            }
            catch (Exception e)
            {
                return $"Failed to set property {_member.Name}: {e.Message}";
            }
        }

        public override SingleTypeOfValue ReturnType => new(_guessedValueType);
        public override bool IsReflected => true;
        public override bool IsSettable { get; }
        public override string? Description => _description;
    }
}
