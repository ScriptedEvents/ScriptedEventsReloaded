using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.ValueTokens;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.Arguments;

public class FloatArgument : Argument
{
    private readonly float? _maxValue;
    private readonly float? _minValue;
    private readonly bool _preferPercent;

    public FloatArgument(
        string name,
        float? minValue = null,
        float? maxValue = null,
        bool preferPercent = false) : base(name)
    {
        if (minValue.HasValue && maxValue.HasValue && minValue.Value > maxValue.Value)
        {
            throw new AndrzejFuckedUpException(
                $"{nameof(FloatArgument)} has minValue at {minValue.Value} and maxValue at {maxValue.Value}.");
        }

        _minValue = minValue;
        _maxValue = maxValue;
        _preferPercent = preferPercent;
    }

    public override string InputDescription =>
        (_minValue.HasValue, _maxValue.HasValue) switch
        {
            (true, true) => $"A number between {FormatNum(_minValue)} and {FormatNum(_maxValue)} (inclusive)",
            (true, false) => $"A number bigger or equal {FormatNum(_minValue)}",
            (false, true) => $"A number smaller or equal {FormatNum(_maxValue)}",
            _ => "Any number"
        };

    private string FormatNum(double? number)
    {
        if (!_preferPercent) return number!.Value.ToString();
        return $"{number!.Value * 100}%";
    }

    [UsedImplicitly]
    public DynamicTryGet<float> GetConvertSolution(BaseToken token)
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

    private TryGet<float> VerifyRange(NumberValue value)
    {
        var result = (float)value.Value;
        if (result < _minValue)
            return $"Value {value} is lower than allowed minimum value {_minValue}.";

        if (result > _maxValue)
            return $"Value {value} is higher than allowed maximum value {_maxValue}.";

        return result;
    }
}