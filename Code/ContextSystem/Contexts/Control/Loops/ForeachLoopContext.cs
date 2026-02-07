using JetBrains.Annotations;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers.Documentation;
using SER.Code.Helpers.ResultSystem;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.MethodSystem.Methods.BroadcastMethods;
using SER.Code.MethodSystem.Methods.OutputMethods;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.ContextSystem.Contexts.Control.Loops;

[UsedImplicitly]
public partial class ForeachLoopContext
{
    public override string KeywordName => "foreach";
    
    public override string Description =>
        "Repeats its body for each player in the player variable or a value in a collection variable, " +
        "assigning it its own custom variable.";
    
    public override string[] Arguments => ["[player/collection variable]"];

    public override DocComponent[] ExampleUsage =>
    [
        new DocComment("The 'foreach' will repeat the methods inside its body the same amount of times as there are provided players"),
        new DocComment("For example, if there are 5 players on the server, 'detected player' will be printed 5 times"),
        GetDoc(
            PlayerVariableToken.GetToken("@all"),
            null,
            null,
            new DocMethod<PrintMethod>(
                BaseToken.GetToken<TextToken>("\"detected player\"")
            )
        ),
        new DocComment("But we can also check which player we are currently 'going over'"),
        new DocComment("The order in which that happens is pretty much random"),
        new DocComment("For example, what will happen if there are 3 players: Player1, Player2 and Player3?"),
        new DocComment("It will send a broadcast to each player with their specific name!"),
        GetDoc(
            PlayerVariableToken.GetToken("@all"),
            PlayerVariableToken.GetToken("@plr"),
            null,
            new DocMethod<BroadcastMethod>(
                PlayerVariableToken.GetToken("@plr"),
                DurationToken.GetToken("10s"),
                TextToken.GetToken("Hello {@plr name} on our server!")
            )
        ),
        new DocComment("This loop supports 1 more argument: the 'position' of the player")
    ];

    public TypeOfValue[] OptionalVariableTypes =>
    [
        new TypesOfValue(new TypeOfValue<PlayerValue>(), new TypeOfValue<ReferenceValue>()),
        new TypeOfValue<NumberValue>()
    ];

    public override Dictionary<IExtendableStatement.Signal, Func<IEnumerator<float>>> RegisteredSignals { get; } = new();

    protected override string FriendlyName => "'foreach' loop statement";

    public static DocStatement GetDoc<TIteratingVar>(
        TIteratingVar iteratingVariable, 
        PlayerVariableToken? playerVar, 
        LiteralVariableToken? indexVar,
        params DocComponent[] body
    )
    where TIteratingVar : VariableToken, ITraversableValueToken, new()
    {
        return new DocStatement("foreach", iteratingVariable)
            .AddRangeIf(() =>
            {
                List<VariableToken> vars = [];
                if (playerVar != null) vars.Add(playerVar);
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
}

public partial class ForeachLoopContext : LoopContext, IAcceptOptionalVariableDefinitions
{
    private readonly Result _mainErr = "Cannot create 'foreach' loop.";
    
    private VariableToken? _indexIterationVariableToken;
    private Variable? _indexIterationVariable;
    private VariableToken? _itemIterationVariableToken;
    private Variable? _itemIterationVariable;
    
    private Func<Value[]>? _values = null;

    protected override TryAddTokenRes OnAddingToken(BaseToken token)
    {
        if (token is not ITraversableValueToken valToken)
        {
            return TryAddTokenRes.Error(
                "'foreach' loop expected to have either a player value or collection value as its third argument, " +
                $"but received '{token.RawRep}'."
            );
        }

        _values = () =>
        {
            if (valToken.GetTraversableValues().HasErrored(out var error, out var value))
            {
                throw new ScriptRuntimeError(this, $"Cannot get value from '{token.RawRep}': {error}");
            }

            return value;
        };

        return TryAddTokenRes.End();
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

            if (_itemIterationVariable is not null) Script.RemoveVariable(_itemIterationVariable);
            if (_indexIterationVariable is not null) Script.RemoveVariable(_indexIterationVariable);

            if (ReceivedBreak)
            {
                break;
            }
        }
    }
}