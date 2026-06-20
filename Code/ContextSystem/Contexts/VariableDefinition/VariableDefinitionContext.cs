using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.ContextSystem.Contexts.VariableDefinition;

public abstract class VariableDefinitionContext : YieldingContext
{
    public bool CreateLocalVariable = true;
    public Variable? DefinedVariable;
}

public abstract class VariableDefinitionContext<TVarToken, TValue, TVariable>(TVarToken varToken) : VariableDefinitionContext
    where TVarToken : VariableToken<TVariable, TValue>
    where TValue : Value
    where TVariable : Variable<TValue>
{
    protected bool EqualSignSet = false;
    protected ValueExpressionContext? Expression = null;

    public override string FriendlyName => $"'{varToken.RawRep}' variable definition";

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        if (!EqualSignSet)
        {
            if (token is not SymbolToken { RawRep: "=" })
            {
                return TryAddTokenRes.Error(
                    "After a variable, an equals sign is expected to set a value to said variable.");
            }

            EqualSignSet = true;
            return TryAddTokenRes.Continue();
        }

        if (Expression == null)
        {
            Expression = new ValueExpressionContext(token, true)
            {
                Script = Script,
                ParentContext = this
            };
            return TryAddTokenRes.Continue();
        }

        return Expression.TryAddToken(token);
    }

    public override OldResult VerifyCurrentState()
    {
        if (!EqualSignSet) return $"Value for variable '{varToken.RawRep}' was not provided (missing equals sign).";
        if (Expression is null) return $"Value for variable '{varToken.RawRep}' was not provided.";
        return Expression.VerifyCurrentState();
    }

    protected override IEnumerator<float> Execute()
    {
        if (Expression is null) throw new AndrzejFuckedUpException();

        var coro = Expression.Run();
        while (coro.MoveNext())
        {
            yield return coro.Current;
        }

        if (Expression.GetValue().SuccessTryCast<TValue>().HasErrored(out var error, out var tValue))
        {
            throw new ScriptRuntimeError(this,
                $"Value returned by {FriendlyName} cannot be assigned to the '{varToken.RawRep}' variable: {error}"
            );
        }

        DefinedVariable = Variable.Create(varToken.Name, tValue);

        if (CreateLocalVariable)
            Script.AddLocalVariable(DefinedVariable);
    }
}