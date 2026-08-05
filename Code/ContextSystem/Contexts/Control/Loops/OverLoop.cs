using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.ContextSystem.Contexts.Control.Loops;

[UsedImplicitly]
public class OverLoop : LoopContext, IAcceptOptionalVariableDefinitionsContext
{
    private readonly Result _mainErr = "Cannot create 'over' loop.";
    private Variable? _indexIterationVariable;
    private VariableToken? _indexIterationVariableToken;
    private Variable? _itemIterationVariable;
    private VariableToken? _itemIterationVariableToken;
    private IValueToken? _itemIterationValueToken;
    
    public override string KeywordName => "over";
    public override string Description =>
        "Repeats its body for each player in the player variable or a value in a collection variable, " +
        "assigning it its own custom variable.";
    public override ContextArgument[] Arguments => [ContextArgument.Required(
        "@players", "Players or collection whose values are iterated.",
        "A player value or collection value."), ContextArgument.Optional(
        "with @item [$index]", "Names variables receiving the current item and optional 1-based index.",
        "One or two variables compatible with the iterated value.")];

    protected override string DetailedUsage =>
        """
        # Run once for every player.
        over @all
            Print "found player"
        end

        # Bind the current item to a temporary variable.
        over @all with @plr
            Print "found player {@plr -> name}"
        end

        # Collections can be iterated in the same way.
        &inventory = @sender -> inventory
        over &inventory with *item
            Print "found item {*item -> type}"
        end

        # Add a 1-based index as a second binding.
        over @all with @plr $index
            Print "found player #{$index}: {@plr -> name}"
        end
        """;

    public Result SetOptionalVariables(params VariableToken[] variableTokens)
    {
        if (variableTokens.Length > 2)
            return $"Too many arguments were provided for '{KeywordName}' loop, only 2 are allowed.";

        if (variableTokens.FirstOrDefault() is not { } itemToken) return true;

        _itemIterationVariableToken = itemToken;

        if (variableTokens.LastOrDefault() is not { } indexToken || indexToken == itemToken) return true;

        if (!indexToken.ValueType.CanHold<NumberValue>())
        {
            return $"Provided variable '{indexToken.RawRep}' cannot be used for this loop, " +
                   $"as it cannot hold a {typeof(NumberValue).FriendlyTypeName()}";
        }

        _indexIterationVariableToken = indexToken;
        return true;
    }

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        if (token is not IValueToken valToken || valToken.NotCapableOf<PlayerValue, CollectionValue>())
        {
            return TryAddTokenRes.Error(
                "'over' loop expected to have either a player value or collection value as its argument, " +
                $"but received '{token.RawRep}'."
            );
        }

        _itemIterationValueToken = valToken;
        return TryAddTokenRes.End();
    }

    public override Result VerifyCurrentState()
    {
        return Result.Assert(
            _itemIterationValueToken is not null,
            _mainErr + "Missing required arguments.");
    }

    protected override IEnumerator<float> Execute()
    {
        if (_itemIterationValueToken is null) throw new CoreInvariantException();

        if (_itemIterationValueToken.Value().HasErrored(out var error, out var dirtyValue))
        {
            throw new ScriptRuntimeError(this, error);
        }

        var values = dirtyValue switch
        {
            PlayerValue players => players.Players.Select(player => new PlayerValue(player)).ToArray<Value>(),
            CollectionValue collection => collection.CastedValues,
            _ => throw new ScriptRuntimeError(
                this,
                $"Value '{dirtyValue}' cannot be iterated over by an 'over' loop."
            )
        };

        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index];

            if (_itemIterationVariableToken is not null)
            {
                _itemIterationVariable = Variable.Create(_itemIterationVariableToken.Name, value);
                Script.AddLocalVariable(_itemIterationVariable);
            }

            if (_indexIterationVariableToken is not null)
            {
                _indexIterationVariable = Variable.Create(_indexIterationVariableToken.Name, new NumberValue(index + 1));
                Script.AddLocalVariable(_indexIterationVariable);
            }

            using var coro = RunChildren();
            while (coro.MoveNext())
            {
                yield return coro.Current;
            }

            if (_itemIterationVariable is not null) Script.RemoveLocalVariable(_itemIterationVariable);
            if (_indexIterationVariable is not null) Script.RemoveLocalVariable(_indexIterationVariable);

            if (ReceivedBreak)
            {
                break;
            }
        }
    }
}
