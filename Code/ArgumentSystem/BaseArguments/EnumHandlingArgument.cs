using SER.Code.ArgumentSystem.Arguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.BaseArguments;

public abstract class EnumHandlingArgument(string name) : Argument(name)
{
    public DynamicTryGet<T> ResolveEnums<T>(
        BaseToken token,
        Dictionary<Type, Func<object, DynamicTryGet<T>>> handlers,
        Func<DynamicTryGet<T>> fallback,
        Func<T?>? fallbackForDynamicEnumResolving = null)
    {
        if (InternalEnumResolve() is { } enumResult)
        {
            return enumResult;
        }

        foreach (var enumType in handlers.Keys)
        {
            if (EnumArgument.ConvertOne(token.BestStaticTextRepr(), enumType)
                .HasErrored(out _, out var enumValue))
            {
                continue;
            }

            var dynamicGet = handlers[enumType](enumValue);
            if (!dynamicGet.Static)
            {
                return dynamicGet;
            }
        }

        var result = fallback();
        if (!result.Static)
        {
            return result;
        }

        if (!result.Invoke().HasErrored(out var err, out var value))
        {
            return value;
        }

        if (!token.CanReturn<LiteralValue>(out _))
        {
            return err;
        }

        return new(() => DynamicEnumParse());

        TryGet<T> DynamicEnumParse()
        {
            if (InternalEnumResolve() is { } enumResult2)
            {
                return enumResult2;
            }
            
            if (fallbackForDynamicEnumResolving != null && fallbackForDynamicEnumResolving() is { } value2)
            {
                return value2;
            }

            return err;
        }

        TryGet<T>? InternalEnumResolve()
        {
            foreach (var enumType in handlers.Keys)
            {
                if (EnumArgument.ConvertOne(token.BestStaticTextRepr(), enumType)
                    .HasErrored(out _, out var enumValue))
                {
                    continue;
                }

                var dynamicGet = handlers[enumType](enumValue);

                if (!dynamicGet.Static)
                {
                    return null;
                }

                return dynamicGet.Invoke();
            }

            return null;
        }
    }
}