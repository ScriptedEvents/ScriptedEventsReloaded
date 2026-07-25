using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.ValueTokens;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.Arguments;

public class IntArgument : Argument
{
    private readonly int? _maxValue;
    private readonly int? _minValue;

    public IntArgument(string name, int? minValue = null, int? maxValue = null) : base(name)
    {
        if (minValue.HasValue && maxValue.HasValue && minValue.Value > maxValue.Value)
        {
            throw new AndrzejFuckedUpException(
                $"{nameof(IntArgument)} has minValue at {minValue.Value} and maxValue at {maxValue.Value}.");
        }

        _minValue = minValue;
        _maxValue = maxValue;
    }

    public override string InputDescription => 
        (_minValue.HasValue, _maxValue.HasValue) switch
        {
            (true, true) => $"A whole number between {_minValue} and {_maxValue} (inclusive)",
            (true, false) => $"A whole number bigger or equal {_minValue}",
            (false, true) => $"A whole number smaller or equal {_maxValue}",
            _ => "Any whole number"
        };

    [UsedImplicitly]
    public DynamicTryGet<int> GetConvertSolution(BaseToken token)
    {
        if (token is NumberToken number)
        {
            return VerifyRange(number.Value.Value);
        }

        if (!token.CanReturn<NumberValue>(out var func))
        {
            return $"{token} is not {InputDescription}.";
        }

        return new(() => func().OnSuccess(VerifyRange));
    }

    private TryGet<int> VerifyRange(NumberValue value)
    {
        var result = (int)value.Value;
        if (result < _minValue)
            return $"Value {value} is lower than allowed minimum value {_minValue}.";

        if (result > _maxValue)
            return $"Value {value} is higher than allowed maximum value {_maxValue}.";

        return result;
    }
}