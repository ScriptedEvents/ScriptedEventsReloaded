using SER.Code.ArgumentSystem.Arguments;
using SER.Code.Extensions;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ArgumentSystem.BaseArguments;

public abstract class EnumHandlingArgument(string name) : Argument(name)
{
    protected interface IEnumHandler<TReturn>
    {
        public Type EnumType { get; }
        public Func<object, OldDynamicTryGet<TReturn>> Handler { get; }
    }
    
    protected class EnumHandler<TEnum, TReturn>(Func<TEnum, OldDynamicTryGet<TReturn>> handler) 
        : IEnumHandler<TReturn> where TEnum : struct, Enum
    {
        public Type EnumType { get; } = typeof(TEnum);
        public Func<object, OldDynamicTryGet<TReturn>> Handler { get; } = obj => handler((TEnum) obj);
    }

    /// <summary>
    /// This function automatically handles an argument that has to handle enums and more.
    /// </summary>
    /// <param name="token">The argument token.</param>
    /// <param name="enumHandlers">
    ///     This registers the enum handlers. This will be parsed statically when possible,
    ///     or dynamically when literal variable is detected.
    /// </param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    protected OldDynamicTryGet<T> EnumResolver<T>(
        BaseToken token,
        IEnumHandler<T>[] enumHandlers)
    {
        if (InternalEnumResolve() is { } value1)
        {
            return value1;
        }

        if (!token.CanReturn<LiteralValue>(out _))
        {
            return GenericError(token);
        }

        return new(() =>
        {
            if (InternalEnumResolve() is { } value2)
            {
                return value2.Invoke();
            }

            return GenericError(token);
        });

        OldDynamicTryGet<T>? InternalEnumResolve()
        {
            var stringRep = token.BestStaticTextRepr();
            foreach (var enumHandler in enumHandlers)
            {
                if (EnumArgument.ConvertOne(stringRep, enumHandler.EnumType)
                    .HasErrored(out _, out var enumValue))
                {
                    continue;
                }
                
                return enumHandler.Handler(enumValue);
            }

            return null;
        }
    }
}