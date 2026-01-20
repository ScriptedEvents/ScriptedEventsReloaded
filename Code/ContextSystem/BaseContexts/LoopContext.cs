using SER.Code.ContextSystem.Extensions;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;

namespace SER.Code.ContextSystem.BaseContexts;

public abstract class LoopContext : StatementContext, IExtendableStatement, IKeywordContext
{
    public IExtendableStatement.Signal AllowedSignals => IExtendableStatement.Signal.DidntExecute;
    public abstract Dictionary<IExtendableStatement.Signal, Func<IEnumerator<float>>> RegisteredSignals { get; }
    
    public abstract string KeywordName { get; }
    public abstract string Description { get; }
    public abstract string[] Arguments { get; }

    protected bool ReceivedContinue;
    protected bool ReceivedBreak;
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

    protected override IEnumerator<float> RunChildren()
    {
        foreach (var coro in Children
                     .TakeWhile(_ => !ReceivedBreak)
                     .TakeWhile(_ => Script.IsRunning)
                     .Select(child => child.ExecuteBaseContext()))
        {
            while (coro.MoveNext())
            {
                yield return coro.Current;
            }

            if (!ReceivedContinue) continue;

            ReceivedContinue = false;
            break;
        }
    }
}