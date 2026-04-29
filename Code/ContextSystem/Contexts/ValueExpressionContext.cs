using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Helpers;
using SER.Code.Helpers.ResultSystem;
using SER.Code.MethodSystem.BaseMethods.Interfaces;
using SER.Code.MethodSystem.BaseMethods.Yielding;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;
using SER.Code.ValueSystem.PropertySystem;

namespace SER.Code.ContextSystem.Contexts;

/// <summary>
/// Used to unify method calls, math expressions and property access into a single context that returns a value.
/// </summary>
public class ValueExpressionContext : AdditionalContext
{
    public abstract class Handler
    {
        public abstract TryGet<Value> GetReturnValue();
        public abstract TryAddTokenRes TryAddToken(BaseToken token);
        public abstract Result VerifyCurrentState();
        public abstract IEnumerator<float> Run();
        public abstract string FriendlyName { get; }
        public abstract TypeOfValue PossibleValues { get; }
    }

    private readonly BaseToken _initial;
    private readonly IValueToken? _initialValueToken;
    private readonly string? _error;
    private Handler? _handler;
    
    /// <summary>
    /// Used to unify method calls, math expressions and property access into a single context that returns a value.
    /// </summary>
    public ValueExpressionContext(BaseToken initial, bool allowsYielding)
    {
        _initial = initial;
        try
        {
            switch (initial)
            {
                case MethodToken methodToken:
                    _handler = new MethodHandler(methodToken, allowsYielding, initial.Script);
                    break;
                case RunFunctionToken:
                    _handler = new FunctionCallHandler(initial.Script);
                    break;
                case IValueToken valToken:
                    _initialValueToken = valToken;
                    break;
                default:
                    _error = $"{initial} is not a valid way to get a value.";
                    break;
            }
        }
        catch (Exception e)
        {
            _error = e.Message;
        }
    }

    public override string FriendlyName => _handler?.FriendlyName ?? "value expression";
    
    public TypeOfValue PossibleValues => _handler?.PossibleValues ?? new UnknownTypeOfValue();
    
    public override TryAddTokenRes TryAddToken(BaseToken token) 
    {
        if (_error is not null)
        {
            return TryAddTokenRes.Error(_error);
        }

        if (_handler is not null)
        {
            goto try_add_token;
        }

        if (token is not SymbolToken symbol)
        {
            return TryAddTokenRes.Error($"{token} is not a valid way to get a value with {_initial}");
        }
            
        _handler = symbol switch
        {
            { IsArrow: false } => new NumericExpressionValueHandler(_initial, Script),
            { IsArrow: true } => new ValuePropertyHandler(_initial, (IValueToken)_initial)
        };

        try_add_token:
        return _handler.TryAddToken(token);
    }

    public override Result VerifyCurrentState()
    {
        if (_error is not null) return _error;
        if (_handler is null) return true;
        return _handler.VerifyCurrentState();
    }

    /// <summary>
    /// If the context is not yielding, disregard the return value.
    /// </summary>
    public IEnumerator<float> Run()
    {
        if (_handler is null) yield break;
        var coro = _handler.Run();
        while (coro.MoveNext()) yield return coro.Current;
    }
    
    public TryGet<Value> GetValue() => 
        _handler?.GetReturnValue() 
        ?? _initialValueToken?.Value() 
        ?? throw new AndrzejFuckedUpException();
}

public class MethodHandler : ValueExpressionContext.Handler
{
    private readonly MethodContext _context;

    public MethodHandler(MethodToken token, bool allowsYielding, Script scr)
    {
        _context = new MethodContext(token)
        {
            Script = scr,
            LineNum = null
        };
        var method = token.Method;
        
        if (method is not IReturningMethod) 
            throw new Exception($"Method '{method.Name}' does not return a value.'");
        
        if (method is YieldingMethod && !allowsYielding)
            throw new Exception(
                $"Method '{method.Name}' is yielding, but you cannot use yielding methods in this context. " +
                $"Consider making a variable and using that variable instead.");
    }

    public override string FriendlyName => _context.FriendlyName;
    
    public override TypeOfValue PossibleValues =>
        _context.Returns 
        ?? throw new AndrzejFuckedUpException("Method has no return type.");

    public override TryGet<Value> GetReturnValue()
    {
        if (_context.ReturnedValue is { } value) return value;
        return _context.MissingValueHint;
    }

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        return _context.TryAddToken(token);
    }

    public override Result VerifyCurrentState()
    {
        return _context.VerifyCurrentState();
    }

    public override IEnumerator<float> Run()
    {
        var coro = _context.Run();
        while (coro.MoveNext()) yield return coro.Current;
    }
}

/// <remarks>
/// Keep in mind that this class will also be used for simple value getting, as parameters are not required!
/// </remarks>
public class ValuePropertyHandler(
    BaseToken baseToken, 
    IValueToken valueToken) : ValueExpressionContext.Handler
{
    private readonly PropertyAccess _propertyAccess = new(baseToken, valueToken);

    public override string FriendlyName => "property access";
    public override TypeOfValue PossibleValues => _propertyAccess.PossibleValues;

    public override TryGet<Value> GetReturnValue() => _propertyAccess.ResolveValue();

    public override TryAddTokenRes TryAddToken(BaseToken token) => _propertyAccess.TryAddToken(token);

    public override Result VerifyCurrentState() => Result.Assert(
        _propertyAccess.PropertyCount > 0, 
        $"The '{SymbolToken.Arrow}' operator was used, but no property to be accessed was specified."
    );

    public override IEnumerator<float> Run()
    {
        yield break;
    }
}

/// <summary>
/// Used for math expressions.
/// </summary>
public class NumericExpressionValueHandler(BaseToken initial, Script scr)
    : ValueExpressionContext.Handler
{
    private readonly List<BaseToken> _tokens = [initial];
    private Safe<NumericExpressionReslover.CompiledExpression> _expression;

    public override string FriendlyName => $"numeric expression";

    public override TypeOfValue PossibleValues => new UnknownTypeOfValue();

    public override TryGet<Value> GetReturnValue()
    {
        return _expression.Value.Evaluate().OnSuccess(obj => Value.Parse(obj, scr));
    }

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        _tokens.Add(token);
        return TryAddTokenRes.Continue();
    }

    public override Result VerifyCurrentState()
    {
        if (NumericExpressionReslover.CompileExpression(_tokens.ToArray()).HasErrored(out var error, out var compiledExpression))
        {
            return error;
        }
        
        _expression = compiledExpression;
        return true;
    }

    public override IEnumerator<float> Run()
    {
        yield break;
    }
}

public class FunctionCallHandler(Script scr) : ValueExpressionContext.Handler
{
    private FuncStatement? _func;
    private readonly List<IValueToken> _providedValues = [];

    public override TryGet<Value> GetReturnValue()
    {
        if (_func!.ReturnedValue is { } value) return value;
        return _func.MissingValueHint;
    }

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        if (_func is null)
        {
            if (!scr.DefinedFunctions.TryGetValue(token.RawRep, out var func))
            {
                return TryAddTokenRes.Error($"Function '{token.RawRep}' is not defined.");
            }
            
            _func = func;
            return TryAddTokenRes.Continue();
        }
        
        if (token is IValueToken valToken)
        {
            _providedValues.Add(valToken);
            return TryAddTokenRes.Continue();
        }

        return TryAddTokenRes.Error($"Unexpected token '{token.RawRep}'");
    }

    public override Result VerifyCurrentState()
    {
        return Result.Assert(
            _func is not null, 
            "Function to run was not provided."
        );
    }

    public override IEnumerator<float> Run()
    {
        List<Value> varsToProvide = []; 
        foreach (var valToken in _providedValues)
        {
            if (valToken.Value().HasErrored(out var error, out var variable))
            {
                throw new ScriptRuntimeError(_func!, 
                    $"Cannot run {_func!.FriendlyName}: {error}"
                );
            }
            
            varsToProvide.Add(variable);
        }

        var coro = _func!.RunProperly(varsToProvide.ToArray());
        while (coro.MoveNext()) yield return coro.Current;
    }

    public override string FriendlyName => _func!.FriendlyName;

    public override TypeOfValue PossibleValues =>
        _func?.Returns 
        ?? throw new AndrzejFuckedUpException("Function has no return type.");
}