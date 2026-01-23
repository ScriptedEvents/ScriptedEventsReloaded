using JetBrains.Annotations;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem;

namespace SER.Code.Helpers.Extensions;

public static class SerExtensions
{
    public static TryGet<TOut> SuccessTryCast<TIn, TOut>(this TryGet<TIn> value) where TOut : TIn
    {
        return value.OnSuccess(v => v.TryCast<TIn, TOut>(), null);
    }
    
    public static TryGet<TOut> SuccessTryCast<TOut>(this TryGet<Value> value) where TOut : Value
    {
        return value.OnSuccess(v => v.TryCast<Value, TOut>(), null);
    }
    
    public static TryGet<TOut> TryCast<TIn, TOut>([NotNull] this TIn value, string rawRep = "") where TOut : TIn
    {
        if (value is null) throw new AndrzejFuckedUpException();
        
        if (value is TOut outValue)
        {
            return outValue;
        }

        string valueRep = "";
        if (!string.IsNullOrWhiteSpace(rawRep))
        {
            valueRep = $"A value '{rawRep}' of type ";
        }
        
        return $"{valueRep}{value.FriendlyTypeName()} is not a {typeof(TOut).FriendlyTypeName()}";
    }

    public static bool CanReturn<T>(this BaseToken token, out Func<TryGet<T>> get) where T : Value
    {
        get = null!;
        if (token is not IValueToken valToken) return false;
        return valToken.CanReturn(out get);
    }
    
    public static bool CanReturn<T>(this IValueToken valToken, out Func<TryGet<T>> get) where T : Value
    {
        get = valToken.TryGet<T>;
        
        // if unknown, its always assumed that it may return T
        if (!valToken.PossibleValues.AreKnown(out var knownReturnTypes)) return true;
        
        // if any of known types is assignable to T, or T to type, then it may return T
        return knownReturnTypes.Any(type => typeof(T).IsAssignableFrom(type) || type.IsAssignableFrom(typeof(T)));
    }
    
    public static TryGet<T> TryGet<T>(this BaseToken token) where T : Value
    {
        if (token is not IValueToken valToken) return $"Value '{token.RawRep}' cannot represent a {typeof(T).FriendlyTypeName()}";
        
        return valToken.Value().SuccessTryCast<Value, T>();
    }

    public static TryGet<T> TryGet<T>(this IValueToken valToken) where T : Value
    {
        return valToken.Value().SuccessTryCast<Value, T>();
    }
}