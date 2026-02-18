using JetBrains.Annotations;
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
    public override string KeywordName => "over";
    public override string Description =>
        "Repeats its body for each player in the player variable or a value in a collection variable, " +
        "assigning it its own custom variable.";
    public override string[] Arguments => ["[player/collection variable]"];

    protected override string Usage =>
        """
        # instead of writing something like this:
        repeat {AmountOf @all}
            Print "found player"
        end
        
        # you can use 'over' to do the same:
        over @all
            Print "found player"
        end
        
        # ========================================
        # additionally, "over" loop can tell you which item is currently being iterated over
        # this is usually known as a "for each" loop in other languages
        # this can be done using "with" keyword and naming a temporary variable:
        over @all
            with @plr
            
            Print "found player {@plr name}"
        end
        
        # this also works for collections:
        &inventory = {@sender inventory}
        over &inventory
            with *item
            
            Print "found item {ItemInfo *item type}"
        end
        # its important to remember that the variable type in "with" keyword 
        #  MUST match the value type inside the collection,
        #  if this is not the case, like using $var for a reference value, there will be an error
        
        # ========================================
        # "with" can also define a second variable, which will hold the index of the current item
        # this is a number value starting at 1, and incrementing by 1 for each iteration
        over @all
            with @plr $index
            
            Print "found player #{$index}: {@plr name}"
        end
        """;
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private readonly Result _mainErr = "Cannot create 'over' loop.";
    private VariableToken? _indexIterationVariableToken;
    private Variable? _indexIterationVariable;
    private VariableToken? _itemIterationVariableToken;
    private Variable? _itemIterationVariable;
    
    private Func<Value[]>? _values = null;

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        if (token is not IValueToken valToken)
        {
            goto Error;
        }

        if (valToken.CapableOf<PlayerValue>(out var getPlayer))
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

        if (valToken.CapableOf<CollectionValue>(out var getCollection))
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
            "'over' loop expected to have either a player value or collection value as its third argument, " +
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
            return $"Provided variable '{indexToken.RawRep}' cannot be used for this loop, " +
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
                Script.AddLocalVariable(_itemIterationVariable);
            }

            if (_indexIterationVariableToken is not null)
            {
                _indexIterationVariable = Variable.Create(_indexIterationVariableToken.Name, new NumberValue(index+1));
                Script.AddLocalVariable(_indexIterationVariable);
            }

            var coro = RunChildren();
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