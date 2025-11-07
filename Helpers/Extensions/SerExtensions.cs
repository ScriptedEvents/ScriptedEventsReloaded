using System;
using System.Linq;
using JetBrains.Annotations;
using SER.Helpers.Exceptions;
using SER.Helpers.ResultSystem;
using SER.TokenSystem.Tokens;
using SER.TokenSystem.Tokens.Interfaces;
using SER.ValueSystem;

namespace SER.Helpers.Extensions;

public static class SerExtensions
{
    public static TryGet<TOut> SuccessTryCast<TIn, TOut>(this TryGet<TIn> value) where TOut : TIn
    {
        return value.OnSuccess(v => v.TryCast<TIn, TOut>());
    }
    
    public static TryGet<TOut> SuccessTryCast<TOut>(this TryGet<Value> value) where TOut : Value
    {
        return value.OnSuccess(v => v.TryCast<Value, TOut>());
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
        
        if (valToken.PossibleValueTypes is null) return true;
        return valToken.PossibleValueTypes.Any(type => typeof(T).IsAssignableFrom(type) || type.IsAssignableFrom(typeof(T)));
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