using SER.Code.ArgumentSystem.Arguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.BaseArguments;

public abstract class EnumHandlingArgument(string name) : Argument(name)
{
    protected interface IEnumHandler<TReturn>
    {
        public Type EnumType { get; }
        public Func<object, DynamicTryGet<TReturn>> Handler { get; }
    }
    
    protected class EnumHandler<TEnum, TReturn>(Func<TEnum, DynamicTryGet<TReturn>> handler) 
        : IEnumHandler<TReturn> where TEnum : struct, Enum
    {
        public Type EnumType { get; } = typeof(TEnum);
        public Func<object, DynamicTryGet<TReturn>> Handler { get; } = obj => handler((TEnum) obj);
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
    protected DynamicTryGet<T> EnumResolver<T>(
        BaseToken token,
        IEnumHandler<T>[] enumHandlers)
    {
        if (ResolveEnum(token.BestStaticTextRepr(), enumHandlers) is { } value1)
        {
            return value1;
        }

        if (!token.CanReturn<LiteralValue>(out _))
        {
            return GenericError(token);
        }

        return new(() =>
        {
            if (ResolveEnum(token.BestStaticTextRepr(), enumHandlers) is { } value2)
            {
                return value2.Invoke();
            }

            return GenericError(token);
        });
    }

    /// <summary>
    /// Resolves arguments which accept either an SER value (usually a reference)
    /// or one of several enum representations.
    /// </summary>
    /// <remarks>
    /// The actual value is deliberately inspected at runtime. A capability check
    /// only says that a token may return a type, so it cannot safely select one of
    /// several mutually exclusive conversion paths.
    /// </remarks>
    protected DynamicTryGet<T> ValueOrEnumResolver<T>(
        BaseToken token,
        Func<Value, TryGet<T>> valueHandler,
        IEnumHandler<T>[] enumHandlers)
    {
        if (token is not IValueToken valueToken)
        {
            return EnumResolver(token, enumHandlers);
        }

        if (valueToken.NotCapableOf<ReferenceValue, LiteralValue>())
        {
            return GenericError(token);
        }

        if (!valueToken.IsConstant)
        {
            return new(ResolveDynamicValue);
        }

        if (valueToken.Value().HasErrored(out var error, out var constantValue))
        {
            return error;
        }

        if (constantValue is LiteralValue constantLiteral)
        {
            return ResolveEnum(constantLiteral.StringRep, enumHandlers)
                   ?? GenericError(token);
        }

        return valueHandler(constantValue);

        TryGet<T> ResolveDynamicValue()
        {
            if (valueToken.Value().HasErrored(out var error, out var value))
            {
                return error;
            }

            if (value is not LiteralValue literal)
            {
                return valueHandler(value);
            }

            return ResolveEnum(literal.StringRep, enumHandlers)?.Invoke()
                   ?? GenericError(token);
        }
    }

    private static DynamicTryGet<T>? ResolveEnum<T>(
        string stringRep,
        IEnumerable<IEnumHandler<T>> enumHandlers)
    {
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
