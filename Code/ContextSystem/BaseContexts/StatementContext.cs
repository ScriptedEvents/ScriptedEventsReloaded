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
    
    protected virtual IEnumerator<float> RunChildren()
    {
        foreach (var coro in Children
                     .TakeWhile(_ => Script.IsRunning)
                     .Select(child => child.ExecuteBaseContext()))
        {
            while (coro.MoveNext())
            {
                yield return coro.Current;
            }
        }
    }
}