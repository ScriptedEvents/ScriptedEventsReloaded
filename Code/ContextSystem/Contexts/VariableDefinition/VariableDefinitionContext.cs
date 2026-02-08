using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Extensions;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
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

    protected override string FriendlyName => $"'{varToken.RawRep}' variable definition";

    protected override TryAddTokenRes OnAddingToken(BaseToken token)
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
                    throw new ScriptRuntimeError(this, error);
                }

                return value;
            };
            return TryAddTokenRes.End();
        }

        if (token is IContextableToken contextable && 
            contextable.GetContext(Script) is { } mainContext and IMayReturnValueContext returnValueContext)
        {
            _returnContext = (mainContext, returnValueContext);
            return TryAddTokenRes.Continue();
        }
        
        Log.D("set parser using additional");
        var (result, receivedParser) = AdditionalParsing(token);
        _parser = receivedParser;
        return result;
    }

    public override Result VerifyCurrentState()
    {
        if (_returnContext is {
            main: var main, 
            returner: { Returns: null } returner
        })
        {
            return $"{main} does not return a value. {returner.UndefinedReturnsHint}";
        }
        
        return Result.Assert(
            _returnContext is not null ||
            _parser is not null,
            $"Value for variable '{varToken.RawRep}' was not provided."
        );
    }

    protected override IEnumerator<float> Execute()
    {
        if (Script is null) throw new AnonymousUseException("FunctionDefinitionContext.cs");
        
        if (_returnContext.HasValue)
        {
            var (main, returner) = _returnContext.Value;
            
            var coro = main.ExecuteBaseContext();
            while (coro.MoveNext())
            {
                yield return coro.Current;
            }
            
            Log.D("checking for returned value");
            if (returner.ReturnedValue is not { } value)
            {
                throw new ScriptRuntimeError(this, $"{main} has not returned a value! {returner.MissingValueHint}");
            }

            if (value.TryCast<TValue>().HasErrored(out var error, out var tValue))
            {
                throw new ScriptRuntimeError(this, 
                    $"Value returned by {main} cannot be assigned to the '{varToken.RawRep}' variable: {error}"
                );
            }
        
            Script.AddVariable(Variable.Create(varToken.Name, tValue));
        }
        else if (_parser is not null)
        {
            Script.AddVariable(Variable.Create(varToken.Name, Value.Parse(_parser(), Script)));
        }
        else
        {
            throw new AndrzejFuckedUpException();
        }
    }
}