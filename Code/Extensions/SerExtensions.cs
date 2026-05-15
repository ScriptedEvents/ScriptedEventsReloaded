using System.Diagnostics.CodeAnalysis;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.Exceptions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem;

namespace SER.Code.Extensions;

public static class SerExtensions
{
    public static TryGet<TOut> SuccessTryCast<TIn, TOut>(this TryGet<TIn> value)
        where TIn : notnull
        where TOut : TIn
    {
        return value.OnSuccess(v => v.TryCast<TOut>());
    }
    
    public static TryGet<TOut> SuccessTryCast<TOut>(this TryGet<Value> value) where TOut : Value
    {
        return value.OnSuccess(v => v.TryCast<TOut>());
    }
    
    public static TryGet<TOut> TryCast<TOut>(this object value, string rawRep = "")
    {
        switch (value)
        {
            case null:
                throw new AndrzejFuckedUpException();
            case TOut outValue:
                return outValue;
        }

        string valueRep = "";
        if (!string.IsNullOrWhiteSpace(rawRep))
        {
            valueRep = $"A value '{rawRep}' of type ";
        }
        
        return $"{valueRep}{value.FriendlyTypeName()} is not a {typeof(TOut).FriendlyTypeName()}";
    }

    extension(BaseToken token)
    {
        public bool CanReturn<T>([NotNullWhen(true)] out Func<TryGet<T>>? get) where T : Value
        {
            get = null!;
            if (token is not IValueToken valToken) return false;
            return valToken.CapableOf(out get);
        }
        
        public bool CanReturnReference<T>([NotNullWhen(true)] out Func<TryGet<T>>? get)
        {
            get = null!;
            if (token is not IValueToken valToken) return false;
            if (!valToken.CapableOf<ReferenceValue>(out var refFunc)) return false;

            get = delegate
            {
                if (refFunc().HasErrored(out var error, out var refVal))
                {
                    return error;
                }

                if (ReferenceArgument<T>.TryParse(refVal).HasErrored(out error, out var value))
                {
                    return error;
                }

                return value;
            };
        
            return true;
        }
        
        public TryGet<T> TryGet<T>() where T : Value
        {
            if (token is not IValueToken valToken) return $"Value '{token.RawRep}' cannot represent a {typeof(T).FriendlyTypeName()}";
        
            return valToken.Value().SuccessTryCast<Value, T>();
        }
    }

    extension(IValueToken valToken)
    {
        public bool CapableOf<T>([NotNullWhen(true)] out Func<TryGet<T>>? get) where T : Value
        {
            get = valToken.TryGet<T>;
        
            // if unknown, its always assumed that it may return T
            if (!valToken.PossibleValues.AreKnown(out var knownReturnTypes)) return true;
        
            // if any of known types is assignable to T, or T to type, then it may return T
            return knownReturnTypes.Any(type => typeof(T).IsAssignableFrom(type) || type.IsAssignableFrom(typeof(T)));
        }
        public TryGet<T> TryGet<T>() where T : Value
        {
            return valToken.Value().SuccessTryCast<Value, T>();
        }
    }

    public static PlayerValue ToPlayerValue(this IEnumerable<Player> players)
    {
        return new PlayerValue(players);
    }
}