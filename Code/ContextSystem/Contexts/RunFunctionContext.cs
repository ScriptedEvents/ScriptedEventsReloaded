using JetBrains.Annotations;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem;

namespace SER.Code.ContextSystem.Contexts;

[UsedImplicitly]
public class RunFunctionContext : YieldingContext, IMayReturnValueContext
{
    private FunctionDefinitionContext? _functionDefinitionContext;
    private readonly List<IValueToken> _providedValues = [];

    public TypeOfValue? Returns => _functionDefinitionContext?.Returns;
    public Value? ReturnedValue => _functionDefinitionContext?.ReturnedValue;
    
    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        if (_functionDefinitionContext is null)
        {
            var name = token.GetBestTextRepresentation(Script);
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

    public override Result VerifyCurrentState()
    {
        return Result.Assert(
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
                throw new ScriptRuntimeError(
                    $"Cannot run function '{_functionDefinitionContext!.FunctionName}': {error}"
                );
            }
            
            varsToProvide.Add(variable);
        }

        return _functionDefinitionContext!.RunProperly(varsToProvide.ToArray());
    }
}