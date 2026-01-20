using JetBrains.Annotations;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.CommunicationInterfaces;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.ContextSystem.Contexts.Control.Loops;

[UsedImplicitly]
public class ForeachLoopContext : LoopContext, IAcceptOptionalVariableDefinitions
{
    private readonly Result _mainErr = "Cannot create 'foreach' loop.";
    
    private VariableToken? _indexIterationVariableToken;
    private Variable? _indexIterationVariable;
    private VariableToken? _itemIterationVariableToken;
    private Variable? _itemIterationVariable;
    
    private Func<Value[]>? _values = null;

    public override string KeywordName => "foreach";
    public override string Description =>
        "Repeats its body for each player in the player variable or a value in a collection variable, " +
        "assigning it its own custom variable.";
    public override string[] Arguments => ["[player/collection variable]"];

    public override Dictionary<IExtendableStatement.Signal, Func<IEnumerator<float>>> RegisteredSignals { get; } = new();

    protected override string FriendlyName => "'foreach' loop statement";

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        if (token is not IValueToken valToken)
        {
            goto Error;
        }

        if (valToken.CanReturn<PlayerValue>(out var getPlayer))
        {
            _values = () =>
            {
                if (getPlayer().HasErrored(out var error, out var value))
                {
                    throw new ScriptRuntimeError(this, error);
                }

                return value.Players.Select(p => new PlayerValue(p)).ToArray();
            };
            
            return TryAddTokenRes.End();
        }

        if (valToken.CanReturn<CollectionValue>(out var getCollection))
        {
            _values = () =>
            {
                if (getCollection().HasErrored(out var error, out var value))
                {
                    throw new ScriptRuntimeError(this, error);
                }

                return value.CastedValues;
            };

            return TryAddTokenRes.End();
        }

        Error:
        return TryAddTokenRes.Error(
            "'foreach' loop expected to have either a player value or collection value as its third argument, " +
            $"but received '{token.RawRep}'."
        );
    }

    public override Result VerifyCurrentState()
    {
        return Result.Assert(
            _values is not null,
            _mainErr + "Missing required arguments.");
    }

    public Result SetOptionalVariables(params VariableToken[] variableTokens)
    {
        if (variableTokens.Length > 2)
            return $"Too many arguments were provided for '{KeywordName}' loop, only 2 are allowed.";
        
        if (variableTokens.FirstOrDefault() is not {} itemToken) return true;
        
        _itemIterationVariableToken = itemToken;
        
        if (variableTokens.LastOrDefault() is not {} indexToken || indexToken == itemToken) return true;

        if (!indexToken.ValueType.IsAssignableFrom(typeof(NumberValue)))
        {
            return $"Provided variable '{indexToken.RawRepr}' cannot be used for this loop, " +
                   $"as it cannot hold a {typeof(NumberValue).FriendlyTypeName()}";
        }

        _indexIterationVariableToken = indexToken;
        
        return true;
    }

    protected override IEnumerator<float> Execute()
    {
        if (_values is null) throw new AndrzejFuckedUpException();

        var values = _values();
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index];

            if (_itemIterationVariableToken is not null)
            {
                _itemIterationVariable = Variable.Create(_itemIterationVariableToken.Name, value);
                Script.AddVariable(_itemIterationVariable);
            }

            if (_indexIterationVariableToken is not null)
            {
                _indexIterationVariable = Variable.Create(_indexIterationVariableToken.Name, new NumberValue(index+1));
                Script.AddVariable(_indexIterationVariable);
            }

            var coro = RunChildren();
            while (coro.MoveNext())
            {
                yield return coro.Current;
            }

            if (_itemIterationVariable is not null) Script.RemoveVariable(_itemIterationVariable);
            if (_indexIterationVariable is not null) Script.RemoveVariable(_indexIterationVariable);

            if (ReceivedBreak)
            {
                break;
            }
        }
    }
}