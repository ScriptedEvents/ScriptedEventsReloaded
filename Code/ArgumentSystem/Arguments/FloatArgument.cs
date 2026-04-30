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

    public override string InputDescription
    {
        get
        {
            if (_minValue.HasValue && _maxValue.HasValue)
            {
                return $"A number which is at least {FormatNum(_minValue.Value)} and most {FormatNum(_maxValue.Value)} e.g. " +
                       $"{FormatNum(Math.Round((double)new Random().Next((int)_minValue.Value, (int)_maxValue.Value + 1)))}";
            }

            if (_minValue.HasValue)
            {
                return $"A number which is at least {FormatNum(_minValue.Value)} e.g. {FormatNum(_minValue.Value + 2f)}";
            }

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (_maxValue.HasValue)
            {
                return $"A number which is at most {FormatNum(_maxValue.Value)} e.g. {FormatNum(_maxValue.Value + 1f)}";
            }

            return $"Any number e.g. {FormatNum(1.5)}";
        }
    }

    private string FormatNum(double number)
    {
        if (!_preferPercent) return number.ToString();
        return $"{number * 100}%";
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