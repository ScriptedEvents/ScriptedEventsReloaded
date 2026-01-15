using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Extensions;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.TokenSystem.Structures;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Bases;
using Log = SER.Code.Helpers.Log;

namespace SER.Code.ContextSystem.Contexts.VariableDefinition;

public abstract class VariableDefinitionContext<TVarToken, TValue, TVariable>(TVarToken varToken) : YieldingContext 
    where TVarToken : VariableToken<TVariable, TValue>
    where TValue    : Value
    where TVariable : Variable<TValue>
{
    protected virtual (TryAddTokenRes result, Func<TValue> parser) AdditionalParsing(BaseToken token)
    {
        return (TryAddTokenRes.Error($"Value '{token.RawRep}' ({token.GetType().GetAccurateName()}) cannot be assigned to {typeof(TVarToken).GetAccurateName()} variable"), null!);
    }
    
    protected Func<BaseToken, Func<TValue>?>? AdditionalTokenParser = null;
    
    private bool _equalSignSet = false;
    private (Context main, IMayReturnValueContext returner)? _returnContext = null; 
    private Func<TValue>? _parser = null;
    
    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        if (!_equalSignSet)
        {
            if (token is not SymbolToken { RawRep: "=" })
            {
                return TryAddTokenRes.Error(
                    "After a variable, an equals sign is expected to set a value to said variable.");
            }
            
            _equalSignSet = true;
            return TryAddTokenRes.Continue();
        }

        if (_returnContext != null)
        {
            return _returnContext.Value.main.TryAddToken(token);
        }

        var parser = AdditionalTokenParser?.Invoke(token);
        if (parser != null)
        {
            _parser = parser;
            return TryAddTokenRes.End();
        }

        if (token.CanReturn<TValue>(out var get))
        {
            Log.D("set parser using value capable");
            _parser = () =>
            {
                if (get().HasErrored(out var error, out var value))
                {
                    throw new ScriptRuntimeError(error);
                }

                return value;
            };
            return TryAddTokenRes.End();
        }

        if (token is IContextableToken contextable)
        {
            if (methodToken.Method is not ReturningMethod)
            {
                return TryAddTokenRes.Error($"Method '{token.RawRep}' does not return a value");
            }
            
            var methodContext = new MethodContext(methodToken)
            {
                Script = Script,
                LineNum = LineNum,
            };
            
            _returnContext = (methodContext, methodContext);
            return TryAddTokenRes.Continue();
        }
        
        Log.D("set parser using additional");
        var res = AdditionalParsing(token);
        _parser = res.parser;
        return res.result;
    }

    public override Result VerifyCurrentState()
    {
        return Result.Assert(
            _returnContext is not null ||
            _parser is not null,
            $"Value for variable '{varToken.RawRep}' was not provided."
        );
    }

    protected override IEnumerator<float> Execute()
    {
        if (_returnContext.HasValue)
        {
            var coro = _returnContext.Value.main.ExecuteBaseContext();
            while (coro.MoveNext())
            {
                yield return coro.Current;
            }
            
            Log.D("checking for returned value");
            if (_returnContext.Value.returner.ReturnedValue is not { } value)
            {
                throw new ScriptRuntimeError($"Context {_returnContext.Value.main.Name} has not returned a value!");
            }

            if (value is not TValue tValue)
            {
                throw new ScriptRuntimeError(
                    $"Value returned by '{_returnContext.Value.main.Name}' cannot be assigned to the {varToken.RawRep} variable");
            }
        
            Script.AddVariable(Variable.Create(varToken.Name, Value.Parse(tValue)));
        }
        else if (_parser is not null)
        {
            Script.AddVariable(Variable.Create(varToken.Name, Value.Parse(_parser())));
        }
        else
        {
            throw new AndrzejFuckedUpException();
        }
    }
}