using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.Arguments;

public class IntArgument : Argument
{
    private readonly int? _minValue;
    private readonly int? _maxValue;
    
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

    public override string InputDescription 
    {
        get
        {
            if (_minValue.HasValue && _maxValue.HasValue)
            {
                return $"Value must be at least {_minValue} and at most {_maxValue} e.g. " +
                       $"{UnityEngine.Random.Range(_minValue.Value, _maxValue.Value + 1)}";
            }

            if (_minValue.HasValue)
            {
                return $"Value must be at least {_minValue} e.g. {_minValue + 2}";
            }

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (_maxValue.HasValue)
            {
                return $"Value must be at most {_maxValue} e.g. {_maxValue - 2}";
            }

            return "Any number e.g. 2";
        }
    }
    
    [UsedImplicitly]
    public DynamicTryGet<int> GetConvertSolution(BaseToken token)
    {
        if (token is NumberToken number)
        {
            return VerifyRange(number.Value.Value);
        }
        return new(() => token.TryGetLiteralValue<NumberValue>().OnSuccess(VerifyRange, null));
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