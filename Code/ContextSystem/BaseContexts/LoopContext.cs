using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Helpers;

namespace SER.Code.ContextSystem.BaseContexts;

public abstract class LoopContext : StatementContext, IKeywordContext
{
    protected bool ReceivedBreak;

    protected bool ReceivedContinue;

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
            case Continue:
                ReceivedContinue = true;
                return;
            case Break:
                ReceivedBreak = true;
                return;
            default:
                ParentContext?.SendControlMessage(msg);
                break;
        }
    }

    protected IEnumerator<float> RunChildren()
    {
        foreach (var child in Children)
        {
            if (ReceivedBreak) break;
            
            switch (child)
            {
                case StandardContext standardContext:
                    standardContext.Run();
                    break;

                case YieldingContext yieldingContext:
                {
                    using var coro = yieldingContext.Run();
                    while (coro.MoveNext()) yield return coro.Current;
                    break;
                }

                default:
                    throw new CoreInvariantException("context is not standard nor yielding");
            }

            if (!ReceivedContinue) continue;

            ReceivedContinue = false;
            break;
        }

        WipeEphemeralVariables();
    }
}
