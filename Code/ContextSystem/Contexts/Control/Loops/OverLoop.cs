using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers.Documentation;
using SER.Code.Helpers.ResultSystem;
using SER.Code.MethodSystem.Methods.BroadcastMethods;
using SER.Code.MethodSystem.Methods.ItemMethods;
using SER.Code.MethodSystem.Methods.OutputMethods;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.ExpressionTokens;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.ContextSystem.Contexts.Control.Loops;

[UsedImplicitly]
public class OverLoop : LoopContext, IAcceptOptionalVariableDefinitions
{
    public override string KeywordName => "over";
    
    public override string Description =>
        "Repeats its body for each player in the player variable or a value in a collection variable, " +
        "assigning it its own custom variable.";
    
    public override string[] Arguments { get; } = ["[player/collection variable]"];

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public override DocComponent[] GetExampleUsage()
    {
        var plrPVAR = PlayerVariableToken.GetToken("@plr");
        var allPVAR = PlayerVariableToken.GetToken("@all");
        var duration = DurationToken.GetToken("10s");
        var indexLVAR = LiteralVariableToken.GetToken("$index");
        var itemRVAR = ReferenceVariableToken.GetToken("*item");
        var invPEXP = PlayerExpressionToken.GetToken(plrPVAR, PlayerExpressionToken.PlayerProperty.Inventory);
        return
        [
            new DocComment("The 'over' will repeat the methods inside the same amount of times as there are players"),
            new DocComment("Meaning if there are 5 players on the server, 'detected player' will be printed 5 times"),
            GetDoc(
                allPVAR,
                null,
                null,
                new DocMethod<PrintMethod>(
                    TextToken.GetToken("detected player")
                )
            ),
            new DocLine(),
            new DocComment("But we can also check which player we are currently 'going over'"),
            new DocComment("The order in which that happens is pretty much random"),
            new DocComment("For example, what will happen if there are 3 players: Player1, Player2 and Player3?"),
            new DocComment("It will send a broadcast to each player with their specific name!"),
            GetDoc(
                allPVAR,
                plrPVAR,
                null,
                new DocMethod<BroadcastMethod>(
                    plrPVAR,
                    duration,
                    TextToken.GetToken(
                        "Hello",
                        PlayerExpressionToken.GetToken(plrPVAR, PlayerExpressionToken.PlayerProperty.Name),
                        "on our server!"
                    )
                )
            ),
            new DocLine(),
            new DocComment("This loop supports 1 more argument: the current 'iteration' of the loop"),
            new DocComment("This is usually called an 'index' in programming"),
            GetDoc(
                allPVAR,
                plrPVAR,
                indexLVAR,
                new DocMethod<BroadcastMethod>(
                    plrPVAR,
                    duration,
                    TextToken.GetToken(
                        "Hello",
                        PlayerExpressionToken.GetToken(plrPVAR, PlayerExpressionToken.PlayerProperty.Name),
                        "your index is:",
                        LiteralVariableExpressionToken.GetToken(indexLVAR)
                    )
                )
            ),
            new DocLine(),
            new DocComment("But players are NOT the only thing you can loop over!"),
            new DocComment(
                "If you have a collection value, like",
                invPEXP,
                "so you check each item on its own"
            ),
            GetDoc(
                invPEXP,
                itemRVAR,
                null,
                IfStatement.GetDoc(
                    [
                        new DocMethod<ItemInfoMethod>(
                            true,
                            itemRVAR, 
                            BaseToken.GetToken<BaseToken>("type")
                        )
                    ]
                )
            )
        ];
    }

    public TypeOfValue[] OptionalVariableTypes =>
    [
        new UnknownTypeOfValue(),
        new TypeOfValue<NumberValue>()
    ];

    public override Dictionary<IExtendableStatement.Signal, Func<IEnumerator<float>>> RegisteredSignals { get; } = new();

    public static DocStatement GetDoc<TVal>(
        TVal iteratingValue, 
        VariableToken? itemVar, 
        LiteralVariableToken? indexVar,
        params DocComponent[] body
    )
        where TVal : BaseToken, IValueToken
    {
        return new DocStatement("over", iteratingValue)
            .AddRangeIf(() =>
            {
                List<VariableToken> vars = [];
                if (itemVar != null) vars.Add(itemVar);
                if (indexVar != null) vars.Add(indexVar);

                if (vars.Count is 0) return null;
                return
                [
                    WithContext.GetDoc(vars.ToArray()),
                    new DocSpace()
                ];
            })
            .AddRange(body);
    }
    
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    private readonly Result _mainErr = "Cannot create 'over' loop.";
    
    private VariableToken? _indexIterationVariableToken;
    private Variable? _indexIterationVariable;
    private VariableToken? _itemIterationVariableToken;
    private Variable? _itemIterationVariable;
    
    private Func<Value[]>? _values = null;

    protected override TryAddTokenRes OnAddingToken(BaseToken token)
    {
        if (!token.CanReturn<TraversableValue>(out var func))
        {
            return TryAddTokenRes.Error(
                "'over' loop expected to have either a player value or collection value as its third argument, " +
                $"but received '{token.RawRep}'."
            );
        }

        _values = () =>
        {
            if (func().HasErrored(out var error, out var value))
            {
                throw new ScriptRuntimeError(this, $"Cannot get value from '{token.RawRep}': {error}");
            }

            return value.TraversableValues;
        };

        return TryAddTokenRes.End();
    }

    public override Result VerifyCurrentState()
    {
        return Result.Assert(
            _values is not null,
            _mainErr + "Missing required arguments.");
    }

    Result IAcceptOptionalVariableDefinitions.SetOptionalVariables(params VariableToken[] variableTokens)
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
        if (Script is null) throw new AnonymousUseException("ForeachLoopContext.cs");

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

            if (_itemIterationVariable is not null) Script.RemoveVariable((Variable)_itemIterationVariable);
            if (_indexIterationVariable is not null) Script.RemoveVariable((Variable)_indexIterationVariable);

            if (ReceivedBreak)
            {
                break;
            }
        }
    }
}