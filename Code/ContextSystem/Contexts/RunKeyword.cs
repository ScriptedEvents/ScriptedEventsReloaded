using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;

namespace SER.Code.ContextSystem.Contexts;

[UsedImplicitly]
public class RunKeyword : YieldingContext, IMayReturnValueContext
{
    private readonly List<IValueToken> _providedValues = [];
    private FuncStatement? _functionDefinitionContext;

    public override string FriendlyName =>
        _functionDefinitionContext is not null
            ? $"'{_functionDefinitionContext.FunctionName}' function call"
            : "function call";

    public TypeOfValue? Returns => _functionDefinitionContext?.Returns;
    public Value? ReturnedValue => _functionDefinitionContext?.ReturnedValue;

    public string MissingValueHint => _functionDefinitionContext?.MissingValueHint ?? "Function is not defined.";
    public string UndefinedReturnsHint => _functionDefinitionContext?.UndefinedReturnsHint ?? "Function is not defined.";

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        if (_functionDefinitionContext is null)
        {
            var name = token.BestStaticTextRepr();
            if (!Script.DefinedFunctions.TryGetValue(name, out var func))
            {
                return TryAddTokenRes.Error(
                    $"There is no function with the name '{name}' defined before this usage. " +
                    $"It's important to remember that the function must be ABOVE the place of usage."
                );
            }

            _functionDefinitionContext = func;
        }
        else if (token is IValueToken valToken)
        {
            _providedValues.Add(valToken);
        }
        else
        {
            return TryAddTokenRes.Error($"Unexpected token '{token.RawRep}'");
        }

        return TryAddTokenRes.Continue();
    }

    public override OldResult VerifyCurrentState()
    {
        return OldResult.Assert(
            _functionDefinitionContext != null,
            "Function name was not provided."
        );
    }

    protected override IEnumerator<float> Execute()
    {
        List<Value> varsToProvide = [];
        foreach (var valToken in _providedValues)
        {
            if (valToken.Value().HasErrored(out var error, out var variable))
            {
                throw new ScriptRuntimeError(this,
                    $"Cannot run {_functionDefinitionContext!}: {error}"
                );
            }

            varsToProvide.Add(variable);
        }

        return _functionDefinitionContext!.RunProperly(varsToProvide.ToArray());
    }
}