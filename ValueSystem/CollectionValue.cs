using SER.Helpers.Exceptions;
using SER.Helpers.Extensions;
using SER.Helpers.ResultSystem;
using System.Collections;

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

            if (list.Select(i => i.GetType()).Distinct().Count() > 1)
            {
                throw new ScriptRuntimeError("Collection was detected with mixed types.");
            }

            return field = list.ToArray();
        }
    } = null!;

    public Type Type
    {
        get
        {
            if (CastedValues != Array.Empty<Value>())
            {
                var typeList = CastedValues.ToList().RemoveNulls().Select(i => i.GetType()).Distinct();
                switch (typeList.Count())
                {
                    case > 1:
                        throw new ScriptRuntimeError("Collection was detected with mixed types.");
                    case 1:
                        return field = typeList.First();
                }
            }

            return field;
        }

        set
        {
            if (CastedValues == Array.Empty<Value>()) field = value;
        }
    } = null!;
    
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

    public CollectionValue Insert(object obj)
    {
        Value parsed = Parse(obj);
        var valuesType = Type;
        if (valuesType == parsed.GetType() || valuesType is null)
        {
            return new CollectionValue(CastedValues.Append(parsed));
        }
        else
        {
            throw new ScriptRuntimeError($"Value ({parsed.GetType()}) has to be the same type as the collection ({valuesType}).");
        }
    }

    /// <summary>
    /// Removes every match if <paramref name="amountToRemove"/> is -1
    /// </summary>
    public CollectionValue Remove(Value parsed, int amountToRemove = -1)
    {
        var values = CastedValues.ToList();
        if (parsed.GetType() != Type)
        {
            throw new ScriptRuntimeError($"Value ({parsed.GetType()}) has to be the same type as the collection ({Type}).");
        }

        values.RemoveAll(val =>
        {
            if (val == parsed)
            {
                return amountToRemove > 0 ? amountToRemove-- > 0 : amountToRemove != 0;
            }
            return false;
        });

        return new CollectionValue(values);
    }

    public CollectionValue Remove(object obj, int amountToRemove = -1)
    {
        Value parsed = Parse(obj);
        return Remove(parsed, amountToRemove);
    }

    public CollectionValue RemoveAt(int index)
    {
        return new CollectionValue(CastedValues.Where((_, i) => i != index - 1));
    }

    public static CollectionValue operator +(CollectionValue lhs, CollectionValue rhs)
    {
        if (lhs.Type != rhs.Type)
        {
            throw new ScriptRuntimeError($"Both collections have to be of same type. Provided types: {lhs.Type} and {rhs.Type}");
        }

        return new CollectionValue(lhs.CastedValues.Concat(rhs.CastedValues));
    }

    public static CollectionValue operator -(CollectionValue lhs, CollectionValue rhs)
    {
        if (lhs.Type != rhs.Type)
        {
            throw new ScriptRuntimeError($"Both collections have to be of same type. Provided types: {lhs.Type} and {rhs.Type}");
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