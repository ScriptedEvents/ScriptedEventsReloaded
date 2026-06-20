using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem.Contexts.Control;

[UsedImplicitly]
public class AttemptStatement : StatementContext, IExtendableStatement, IKeywordContext
{
    private Exception? _exception;
    
    public override string FriendlyName => "'attempt' statement";

    public IExtendableStatement.Signal AllowedSignals => IExtendableStatement.Signal.ThrewException;
    public Dictionary<IExtendableStatement.Signal, StatementContext> RegisteredSignals { get; } = [];
    public string KeywordName => "attempt";
    public string Description =>
        "Runs everything inside the statement, and if something throws an exception (error), the error will not " +
        "terminate the script. If there is an 'on_error' statement, it will be executed.";

    public string[] Arguments => [];
    public string Example =>
        """
        &collection = Coll.Empty
        # swallows the error (doesn't stop the script)
        attempt
            Print {Coll.Fetch &collection 2}
            # throws because there's nothing at index 2
        end
        """;

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        return TryAddTokenRes.Error($"A {FriendlyName} does not expect any arguments.");
    }

    public override OldResult VerifyCurrentState()
    {
        return true;
    }

    protected override IEnumerator<float> Execute()
    {
        using IEnumerator<float> coro = RunChildren();
        while (true)
        {
            bool isRunning;
            try
            {
                isRunning = coro.MoveNext();
            }
            catch (Exception ex)
            {
                _exception = ex;
                break;
            }

            if (!isRunning) yield break;
            yield return coro.Current;
        }

        if (!RegisteredSignals.TryGetValue(IExtendableStatement.Signal.ThrewException, out var statement))
            yield break;

        if (statement is OnErrorStatement catchStatement)
        {
            catchStatement.Exception = _exception;
        }
        var catchCoro = statement.Run();
        while (catchCoro.MoveNext())
            yield return catchCoro.Current;
    }
}