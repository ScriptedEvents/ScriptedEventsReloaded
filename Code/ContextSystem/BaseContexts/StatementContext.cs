using SER.Code.ContextSystem.Extensions;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers;

namespace SER.Code.ContextSystem.BaseContexts;

public abstract class StatementContext : YieldingContext
{
    public readonly List<Context> Children = [];
    
    public void SendControlMessage(ParentContextControlMessage msg)
    {
        Log.Debug($"{this} has received control message: {msg}");
        OnReceivedControlMessageFromChild(msg);
    }

    protected virtual void OnReceivedControlMessageFromChild(ParentContextControlMessage msg)
    {
        ParentContext?.SendControlMessage(msg);
    }
    
    protected IEnumerator<float> RunChildren(Func<bool>? endCond = null)
    {
        foreach (var coro in Children.Select(c => c.ExecuteBaseContext()))
        {
            if (endCond?.Invoke() is true) yield break;
            while (coro.MoveNext())
            {
                if (endCond?.Invoke() is true) yield break;
                yield return coro.Current;
            }
        }
    }
}