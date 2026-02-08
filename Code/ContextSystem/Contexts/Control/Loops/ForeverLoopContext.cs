using JetBrains.Annotations;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.Documentation;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;

namespace SER.Code.ContextSystem.Contexts.Control.Loops;

[UsedImplicitly]
public class ForeverLoopContext : LoopContextWithSingleIterationVariable<NumberValue>, IKeywordContext
{
    private readonly Result _mainErr = "Cannot create 'forever' loop.";

    public override Dictionary<IExtendableStatement.Signal, Func<IEnumerator<float>>> RegisteredSignals { get; } = new();
    
    public override string KeywordName => "forever";
    public override string Description => "Makes the code inside the statement run indefinitely.";
    public override string[] Arguments => [];

    public override DocComponent[] GetExampleUsage() => throw new NotImplementedException();

    public static DocStatement GetDoc(params DocComponent[] body)
    {
        return new DocStatement("forever").AddRange(body);
    }

    protected override string FriendlyName => "'forever' loop statement";

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