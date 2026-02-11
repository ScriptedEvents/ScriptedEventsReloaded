using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.Documentation;
using SER.Code.Helpers.ResultSystem;
using SER.Code.MethodSystem.Methods.OutputMethods;
using SER.Code.MethodSystem.Methods.WaitingMethods;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;

namespace SER.Code.ContextSystem.Contexts.Control.Loops;

[UsedImplicitly]
public class ForeverLoop : LoopContextWithSingleIterationVariable<NumberValue>, IKeywordContext
{
    private readonly Result _mainErr = "Cannot create 'forever' loop.";

    public override Dictionary<IExtendableStatement.Signal, Func<IEnumerator<float>>> RegisteredSignals { get; } = new();
    
    public override string KeywordName => "forever";
    public override string Description => "Makes the instructions inside the statement run indefinitely.";
    public override string[] Arguments => [];

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public override DocComponent[] GetExampleUsage() =>
    [
        new DocComment("Instructions inside this block will - you will not believe it - repeat forever!"),
        new DocComment("You can also use a variable to know the current iteration, like a counter"),
        GetDoc(
            LiteralVariableToken.GetToken("$iter"),
            new DocMethod<WaitMethod>(
                DurationToken.GetToken("1s")
            ),
            new DocMethod<PrintMethod>(
                TextToken.GetToken(
                    "Currently at iteration", 
                    LiteralVariableToken.GetToken("$iter")
                )
            )
        )
    ];

    public static DocStatement GetDoc(LiteralVariableToken? iterVar, params DocComponent[] body)
    {
        return new DocStatement("forever", true)
            .AddRangeIf(() =>
            {
                if (iterVar is null) return null;
                return
                [
                    WithContext.GetDoc(iterVar),
                    new DocLine()
                ];
            })
            .AddRange(body);
    }

    protected override TryAddTokenRes OnAddingToken(BaseToken token)
    {
        return TryAddTokenRes.Error(_mainErr + "'forever' loop doesn't expect any arguments.");
    }

    public override Result VerifyCurrentState()
    {
        return true;
    }

    protected override IEnumerator<float> Execute()
    {
        ulong iteration = 0;
        while (true)
        {
            SetVariable(++iteration);
            var coro = RunChildren();
            while (coro.MoveNext())
            {
                yield return coro.Current;
            }
            RemoveVariable();
            
            if (ReceivedBreak) break;
        }
    }
}