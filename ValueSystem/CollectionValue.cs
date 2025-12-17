using SER.Helpers.Exceptions;
using SER.Helpers.Extensions;
using SER.Helpers.ResultSystem;
using System.Collections;
using System.Diagnostics;

namespace SER.ValueSystem;

public class CollectionValue(IEnumerable value) : Value
{
    public Value[] CastedValues
    {
        get
        {
            if (field is not null) return field;

            List<Value> list = [];
            list.AddRange(from object item in value select Parse(item));

            var types = list.Select(i => i.GetType()).Distinct().ToArray();
            if (types.Length > 1)
            {
                throw new ScriptRuntimeError("Collection was detected with mixed types.");
            }
            
            Type = types.FirstOrDefault();
            return field = list.ToArray();
        }
    } = null!;

    /// <summary>
    /// The type of values inside the collection.
    /// Returns null if the collection is empty.
    /// </summary>
    /// <exception cref="ScriptRuntimeError">Collection has mixed types</exception>
    public Type? Type
    {
        get
        {
            if (CastedValues.IsEmpty()) return null;
            if (field is not null) return field;
            
            var types = CastedValues
                .ToList()
                .RemoveNulls()
                .Select(i => i.GetType())
                .Distinct()
                .ToArray();

            return types.Length switch
            {
                > 1 => throw new ScriptRuntimeError("Collection was detected with mixed types."),
                1 => field = types.First(),
                < 1 => throw new Exception("if you see this, the fabric of the universe is collapsing, seek shelter from the darkness")
            };
        }
        private set;
    }

    public override bool EqualCondition(Value other)
    {
        if (other is not CollectionValue otherP || otherP.CastedValues.Length != CastedValues.Length) return false;
        return !CastedValues.Where((val, i) => !val.EqualCondition(otherP.CastedValues[i])).Any();
    }

    public override int HashCode =>
        CastedValues.GetEnumerableHashCode().HasErrored(out var error, out var val)
        ? throw new TosoksFuckedUpException(error)
        : val;

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
        if (collection.Type is not { } type)
        {
            return new CollectionValue(new[] { value });
        }
        
        if (type.IsInstanceOfType(value))
        {
            return new CollectionValue(collection.CastedValues.Append(value));
        }

        throw new ScriptRuntimeError($"Inserted value {value.FriendlyName()} has to be the same type as the collection ({FriendlyName(type)}).");
    }
    public CollectionValue Insert(Value val) => CollectionValue.Insert(this, val);

    /// <summary>
    /// Removes every match if <paramref name="amountToRemove"/> is -1
    /// </summary>
    public static CollectionValue Remove(CollectionValue collection, Value value, int amountToRemove = -1)
    {
        if (collection.Type is not { } type)
        {
            throw new ScriptRuntimeError("Collection is empty");
        }
        
        if (type.IsInstanceOfType(value))
        {
            throw new ScriptRuntimeError($"Value {value.FriendlyName()} has to be the same type as the collection ({FriendlyName(type)}).");
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
    public CollectionValue Remove(Value val, int amountToRemove = -1) => CollectionValue.Remove(this, val, amountToRemove);

    public static CollectionValue RemoveAt(CollectionValue collection, int index)
    {
        return new CollectionValue(collection.CastedValues.Where((_, i) => i != index - 1));
    }
    public CollectionValue RemoveAt(int index) => CollectionValue.RemoveAt(this, index);

    public static bool Contains(CollectionValue collection, Value value) => collection.CastedValues.Contains(value);
    public bool Contains(Value val) => CollectionValue.Contains(this, val);

    public static CollectionValue operator +(CollectionValue lhs, CollectionValue rhs)
    {
        if (lhs.Type != rhs.Type)
        {
            throw new ScriptRuntimeError(
                $"Both collections have to be of same type. " +
                $"Provided types: {lhs.GetType().AccurateName} and {rhs.Type?.AccurateName ?? "none"}"
            );
        }

        return new CollectionValue(lhs.CastedValues.Concat(rhs.CastedValues));
    }

    public static CollectionValue operator -(CollectionValue lhs, CollectionValue rhs)
    {
        if (lhs.Type != rhs.Type)
        {
            throw new ScriptRuntimeError(
                $"Both collections have to be of same type. " +
                $"Provided types: {lhs.Type?.AccurateName ?? "none"} and {rhs.Type?.AccurateName ?? "none"}"
            );
        }

        return new CollectionValue(lhs.CastedValues.Where(val => !rhs.CastedValues.Contains(val)));
    }

    public override string ToString()
    {
        return $"[{string.Join(", ", CastedValues.Select(v => v.ToString()))}]";
    }
}

public class CollectionValue<T>(IEnumerable<T> value) : CollectionValue(value) where T : Value
{
    public new Type Type => typeof(T);
}