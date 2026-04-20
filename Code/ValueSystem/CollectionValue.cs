using System.Collections;
using JetBrains.Annotations;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ValueSystem.PropertySystem;
using ValueType = SER.Code.ValueSystem.Other.ValueType;

namespace SER.Code.ValueSystem;

public class CollectionValue(IEnumerable value) : Value, IValueWithProperties
{
    private static readonly Random Random = new();

    [UsedImplicitly]
    public CollectionValue() : this(Array.Empty<object>()) {}

    public Value[] CastedValues
    {
        get
        {
            if (field is not null) return field;

            List<Value> list = [];
            list.AddRange(from object item in value select Parse(item, null));

            var types = list.Select(i => i.GetType()).Distinct().ToArray();
            if (types.Length > 1)
            {
                var commonBase = types[0];
                for (var i = 1; i < types.Length; i++)
                {
                    var t = types[i];
                    while (commonBase != null && !commonBase.IsAssignableFrom(t))
                    {
                        commonBase = commonBase.BaseType;
                    }
                }

                if (commonBase == null || commonBase == typeof(Value) || commonBase == typeof(object))
                {
                    throw new CustomScriptRuntimeError("Collection was detected with mixed types.");
                }

                StoredTypes = commonBase;
            }
            else
            {
                StoredTypes = types.FirstOrDefault();
            }
            
            return field = list.ToArray();
        }
    } = null!;

    /// <summary>
    /// The type of values inside the collection.
    /// Returns null if the collection is empty.
    /// </summary>
    /// <exception cref="ScriptRuntimeError">Collection has mixed types</exception>
    public Type? StoredTypes
    {
        get
        {
            if (CastedValues.IsEmpty()) return null;
            if (field is not null) return field;
            
            var types = CastedValues
                .Select(i => i.GetType())
                .Distinct()
                .ToArray();

            if (types.Length == 1) return field = types[0];

            var commonBase = types[0];
            for (var i = 1; i < types.Length; i++)
            {
                var t = types[i];
                while (commonBase != null && !commonBase.IsAssignableFrom(t))
                {
                    commonBase = commonBase.BaseType;
                }
            }

            if (commonBase == null || commonBase == typeof(Value) || commonBase == typeof(object))
            {
                throw new CustomScriptRuntimeError("Collection was detected with mixed types.");
            }

            return field = commonBase;
        }
        private set;
    }

    public override bool Equals(Value? other)
    {
        if (other is not CollectionValue otherP || otherP.CastedValues.Length != CastedValues.Length) return false;
        return !CastedValues.Where((val, i) => !val.Equals(otherP.CastedValues[i])).Any();
    }

    public override int HashCode =>
        CastedValues.GetEnumerableHashCode().HasErrored(out var error, out var val)
        ? throw new TosoksFuckedUpException(error)
        : val;
    
    private class Prop<T>(Func<CollectionValue, T> handler, string? description)
        : IValueWithProperties.PropInfo<CollectionValue, T>(handler, description) where T : Value;

    public Dictionary<string, IValueWithProperties.PropInfo> Properties { get; } = new() 
    {
        ["length"] = new Prop<NumberValue>(c => c.CastedValues.Length, "Amount of values in the collection"),
        ["isEmpty"] = new Prop<BoolValue>(c => c.CastedValues.Length == 0, "Whether the collection is empty"),
        ["first"] = new Prop<Value>(c => c.CastedValues.Length > 0 ? c.CastedValues[0] : throw new CustomScriptRuntimeError("Collection is empty"), "First value in the collection"),
        ["last"] = new Prop<Value>(c => c.CastedValues.Length > 0 ? c.CastedValues[^1] : throw new CustomScriptRuntimeError("Collection is empty"), "Last value in the collection"),
        ["random"] = new Prop<Value>(c => c.CastedValues.Length > 0 ? c.CastedValues[Random.Next(c.CastedValues.Length)] : throw new CustomScriptRuntimeError("Collection is empty"), "Random value from the collection"),
        ["sum"] = new Prop<NumberValue>(c => c.CastedValues.OfType<NumberValue>().Sum(n => n.Value), "Sum of all numbers in the collection"),
        ["average"] = new Prop<NumberValue>(c => c.CastedValues.OfType<NumberValue>().Any() ? c.CastedValues.OfType<NumberValue>().Average(n => n.Value) : 0m, "Average of all numbers in the collection"),
        ["valType"] = new Prop<EnumValue<ValueType>>(_ => ValueType.Collection, "The type of the value")
    };

    public TryGet<Value> GetAt(int index)
    {
        if (index < 1) return $"Provided index {index}, but index cannot be less than 1";
        
        try
        {
            return CastedValues[index - 1];
        }
        catch (IndexOutOfRangeException)
        {
            return $"There is no value at index {index}";
        }
    }

    public static CollectionValue Insert(CollectionValue collection, Value value)
    {
        if (collection.StoredTypes is not { } type)
        {
            return new CollectionValue(new[] { value });
        }
        
        if (type.IsInstanceOfType(value))
        {
            return new CollectionValue(collection.CastedValues.Append(value));
        }

        throw new CustomScriptRuntimeError(
            $"Inserted value '{value}' has to be the same type as the collection ({GetFriendlyName(type)})."
        );
    }
    public CollectionValue Insert(Value val) => Insert(this, val);

    /// <summary>
    /// Removes every match if <paramref name="amountToRemove"/> is -1
    /// </summary>
    public static CollectionValue Remove(CollectionValue collection, Value value, int amountToRemove = -1)
    {
        if (collection.StoredTypes is not { } type)
        {
            throw new CustomScriptRuntimeError("Collection is empty");
        }
        
        if (type.IsInstanceOfType(value))
        {
            throw new CustomScriptRuntimeError($"Value {value.FriendlyName} has to be the same type as the collection ({GetFriendlyName(type)}).");
        }

        var values = collection.CastedValues.ToList();
        values.RemoveAll(val =>
        {
            if (val != value)
            {
                return false;
            }
            
            return amountToRemove-- > 0;
        });

        return new CollectionValue(values);
    }
    public CollectionValue Remove(Value val, int amountToRemove = -1) => Remove(this, val, amountToRemove);

    public static CollectionValue RemoveAt(CollectionValue collection, int index)
    {
        return new CollectionValue(collection.CastedValues.Where((_, i) => i != index - 1));
    }
    public CollectionValue RemoveAt(int index) => RemoveAt(this, index);

    public static bool Contains(CollectionValue collection, Value value) => collection.CastedValues.Contains(value);
    public bool Contains(Value val) => Contains(this, val);

    public static CollectionValue operator +(CollectionValue lhs, CollectionValue rhs)
    {
        if (lhs.StoredTypes != rhs.StoredTypes)
        {
            throw new CustomScriptRuntimeError(
                $"Both collections have to be of same type. " +
                $"Provided types: {lhs.GetType().AccurateName} and {rhs.StoredTypes?.AccurateName ?? "none"}"
            );
        }

        return new CollectionValue(lhs.CastedValues.Concat(rhs.CastedValues));
    }

    public static CollectionValue operator -(CollectionValue lhs, CollectionValue rhs)
    {
        if (lhs.StoredTypes != rhs.StoredTypes)
        {
            throw new CustomScriptRuntimeError(
                $"Both collections have to be of same type. " +
                $"Provided types: {lhs.StoredTypes?.AccurateName ?? "none"} and {rhs.StoredTypes?.AccurateName ?? "none"}"
            );
        }

        return new CollectionValue(lhs.CastedValues.Where(val => !rhs.CastedValues.Contains(val)));
    }

    [UsedImplicitly]
    public new static string FriendlyName = "collection value";

    public override string ToString()
    {
        return $"[{string.Join(", ", CastedValues.Select(v => v.ToString()))}]";
    }

    public override TryGet<object> ToCSharpObject(Type targetType)
    {
        if (targetType.IsInstanceOfType(value)) return TryGet<object>.Success(value);

        Type? elementType = null;
        if (targetType.IsArray)
        {
            elementType = targetType.GetElementType();
        }
        else if (targetType.IsGenericType && (targetType.GetGenericTypeDefinition() == typeof(IEnumerable<>) || 
                                              targetType.GetGenericTypeDefinition() == typeof(List<>) ||
                                              targetType.GetGenericTypeDefinition() == typeof(IList<>) ||
                                              targetType.GetGenericTypeDefinition() == typeof(ICollection<>)))
        {
            elementType = targetType.GetGenericArguments()[0];
        }

        if (elementType == null) return $"Cannot convert collection to {targetType.Name}";

        var listType = typeof(List<>).MakeGenericType(elementType);
        var list = (IList)Activator.CreateInstance(listType);

        foreach (var val in CastedValues)
        {
            var converted = val.ToCSharpObject(elementType);
            if (converted.HasErrored(out var error, out var obj)) return error;
            list.Add(obj);
        }

        if (targetType.IsArray)
        {
            var array = Array.CreateInstance(elementType, list.Count);
            list.CopyTo(array, 0);
            return TryGet<object>.Success(array);
        }

        return TryGet<object>.Success(list);
    }
}

[UsedImplicitly]
public class CollectionValue<T>(IEnumerable value) : CollectionValue(value)
{
    [UsedImplicitly]
    public CollectionValue() : this(Array.Empty<T>()) {}
    
    [UsedImplicitly]
    public new static string FriendlyName = $"collection of {(typeof(T).IsSubclassOf(typeof(Value)) 
                ? GetFriendlyName(typeof(T)) 
                : typeof(T).AccurateName)}";
}