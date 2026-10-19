using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem.Contexts.Control;

[UsedImplicitly]
public class AttemptStatement : StatementContext, IExtendableStatement, IKeywordContext
{
    public override string FriendlyName => "'attempt' statement";

    public IExtendableStatement.Signal AllowedSignals => IExtendableStatement.Signal.ThrewException;
    public Dictionary<IExtendableStatement.Signal, StatementContext> RegisteredSignals { get; } = [];
    public string KeywordName => "attempt";
    public string Description =>
        "Runs everything inside the statement, and if something throws an exception (error), the error will not " +
        "terminate the script. If there is an 'on_error' statement, it will be executed.";

    public ContextArgument[] Arguments => [];
    public string Example =>
        """
        &collection = Coll.Empty
        attempt
            Print {Coll.Fetch &collection 2}
        end

        # The error is handled by on_error when one is present.
        on_error with $message
            Print "Handled: {$message}"
        end
        """;

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        return TryAddTokenRes.Error($"A {FriendlyName} does not expect any arguments.");
    }

    public override Result VerifyCurrentState()
    {
        return true;
    }

    protected override IEnumerator<float> Execute()
    {
        // Kept per run (not as a field), so an attempt inside a function that calls itself
        // cannot mix up the errors of different calls.
        Exception? exception = null;
        IEnumerator<float>? coro = null;
        try
        {
            while (true)
            {
                bool isRunning;
                try
                {
                    // Starting the body can already run its first lines, so that also has to be inside the try.
                    coro ??= RunChildren();
                    isRunning = coro.MoveNext();
                }
                catch (Exception ex) when (CanHandle(ex))
                {
                    exception = ex;
                    break;
                }

                if (!isRunning) yield break;
                yield return coro.Current;
            }
        }
        finally
        {
            coro?.Dispose();
        }

        if (!RegisteredSignals.TryGetValue(IExtendableStatement.Signal.ThrewException, out var statement))
            yield break;

        if (statement is OnErrorStatement catchStatement)
        {
            catchStatement.Exception = exception;
        }
        using var catchCoro = statement.Run();
        while (catchCoro.MoveNext())
            yield return catchCoro.Current;
    }

    internal static bool CanHandle(Exception exception) => exception is ScriptRuntimeError;
}
