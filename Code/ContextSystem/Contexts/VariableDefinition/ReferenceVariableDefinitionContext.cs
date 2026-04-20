using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.PropertySystem;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.ContextSystem.Contexts.VariableDefinition;

public class ReferenceVariableDefinitionContext(VariableToken<ReferenceVariable, ReferenceValue> varToken) :
    VariableDefinitionContext<VariableToken<ReferenceVariable, ReferenceValue>, ReferenceValue, ReferenceVariable>(varToken)
{
    private readonly VariableToken<ReferenceVariable, ReferenceValue> _varToken = varToken;
    private PropertyAccess? _propertyAccess;

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        if (token is SymbolToken { IsArrow: true } && !EqualSignSet)
        {
            _propertyAccess ??= new PropertyAccess(_varToken, _varToken);
            return _propertyAccess.TryAddToken(token);
        }

        if (_propertyAccess is not null && !EqualSignSet)
        {
            if (token is SymbolToken { RawRep: "=" })
            {
                EqualSignSet = true;
                return TryAddTokenRes.Continue();
            }

            return _propertyAccess.TryAddToken(token);
        }

        return base.TryAddToken(token);
    }

    public override Result VerifyCurrentState()
    {
        if (_propertyAccess is not null)
        {
            if (!EqualSignSet) return $"Value for property assignment '{_propertyAccess.ExprRepr}' was not provided (missing equals sign).";
            if (_propertyAccess.PropertyCount == 0) return "No properties specified for property assignment.";
            if (Expression is null) return $"Value for property assignment '{_propertyAccess.ExprRepr}' was not provided.";
            return Expression.VerifyCurrentState();
        }

        return base.VerifyCurrentState();
    }

    protected override IEnumerator<float> Execute()
    {
        if (_propertyAccess is null)
        {
            var coro = base.Execute();
            while (coro.MoveNext()) yield return coro.Current;
            yield break;
        }

        if (Expression is null) throw new AndrzejFuckedUpException();

        var coroExpr = Expression.Run();
        while (coroExpr.MoveNext())
        {
            yield return coroExpr.Current;
        }

        if (Expression.GetValue().HasErrored(out var error, out var tValue))
        {
            throw new ScriptRuntimeError(this, 
                $"Value returned by {FriendlyName} could not be resolved: {error}"
            );
        }

        var lastPropRes = _propertyAccess.ResolveLastProp();
        if (lastPropRes.HasErrored(out var lastPropError, out var lastProp))
        {
            throw new ScriptRuntimeError(this, $"Failed to resolve property for assignment: {lastPropError}");
        }

        if (!lastProp.propInfo.IsSettable)
        {
             throw new ScriptRuntimeError(this, $"Property '{lastProp.propInfo.ReturnType}' is read-only.");
        }

        var setResult = lastProp.propInfo.SetValue(lastProp.target, tValue);
        if (setResult.HasErrored(out var setError))
        {
            throw new ScriptRuntimeError(this, $"Failed to set property: {setError}");
        }
    }
}