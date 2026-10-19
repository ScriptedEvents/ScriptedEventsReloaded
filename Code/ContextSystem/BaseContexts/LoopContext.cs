using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers;

namespace SER.Code.ContextSystem.BaseContexts;

public abstract class LoopContext : StatementContext, IKeywordContext
{
    protected abstract string? DetailedUsage { get; }

    public sealed override string FriendlyName => $"'{KeywordName}' loop statement";
    public Dictionary<IExtendableStatement.Signal, StatementContext> RegisteredSignals { get; } = [];

    public abstract string KeywordName { get; }
    public abstract string Description { get; }
    public abstract ContextArgument[] Arguments { get; }

    public string Example => ExampleHandler.GetExample($"{KeywordName}KeywordExample") ??
                              $"""
                               {DetailedUsage}

                               # Use break to leave the loop or continue to skip to its next iteration.
                               """;

    protected override void OnReceivedControlMessageFromChild(ParentContextControlMessage msg)
    {
        switch (msg)
        {
            // 'continue' and 'break' belong to this loop. The current iteration has already stopped,
            // and the loop checks the iteration's StoppedBy to decide whether to go on.
            case Continue:
            case Break:
                return;
            default:
                // 'return' also ends the loop, but the enclosing function needs to receive it too.
                ParentContext?.SendControlMessage(msg);
                break;
        }
    }

    /// <summary>
    ///     Returns whether the loop must stop after an iteration that was stopped this way.
    /// </summary>
    protected static bool EndsLoop(BodyRun iteration)
    {
        return iteration.StoppedBy is Break or Return;
    }
}
