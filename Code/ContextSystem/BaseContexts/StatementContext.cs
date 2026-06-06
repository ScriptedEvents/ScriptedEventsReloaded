using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.ContextSystem.BaseContexts;

public abstract class StatementContext : YieldingContext
{
    public readonly List<RunnableContext> Children = [];
    public readonly HashSet<Variable> EphemeralVariables = [];
    public uint? EndLine;

    public void SendControlMessage(ParentContextControlMessage msg)
    {
        Log.Debug($"{this} has received control message: {msg}");
        OnReceivedControlMessageFromChild(msg);
    }

    protected virtual void OnReceivedControlMessageFromChild(ParentContextControlMessage msg)
    {
        ParentContext?.SendControlMessage(msg);
    }

    [MustDisposeResource]
    protected IEnumerator<float> RunChildren(Func<bool>? endCond = null)
    {
        // If we can run the whole thing synchronously right now, do it.
        // This still bypasses allocating OUR state machine.
        return RunChildrenInternal(endCond) ?? Enumerable.Empty<float>().GetEnumerator(); 
        // optimization tip: Use a cached, static empty enumerator instance instead of Enumerable.Empty
    }
    
    // The compiler only builds a state machine for this helper method
    private IEnumerator<float>? RunChildrenInternal(Func<bool>? endCond)
    {
        for (int i = 0; i < Children.Count; i++)
        {
            if (endCond?.Invoke() is true) goto leave;

            var child = Children[i];
            switch (child)
            {
                case StandardContext sc:
                    sc.Run();
                    continue;

                case YieldingContext:
                    // We hit a yielding context, so we MUST yield.
                    // We pass the remaining index so we know where to resume.
                    return RunChildrenYielding(i, endCond);
            }
        }

        leave:
        WipeEphemeralVariables();
        return null;
    }

    // This handles the heavy lifting ONLY when a yield is actually encountered
    private IEnumerator<float> RunChildrenYielding(int startIndex, Func<bool>? endCond)
    {
        for (int i = startIndex; i < Children.Count; i++)
        {
            if (endCond?.Invoke() is true) goto leave;

            var child = Children[i];
            switch (child)
            {
                case StandardContext sc:
                    sc.Run();
                    continue;

                case YieldingContext yc:
                    var coro = yc.Run();
                    while (coro.MoveNext())
                    {
                        if (endCond?.Invoke() is true) goto leave;
                        yield return coro.Current;
                    }
                    break;
            }
        }

        leave:
        WipeEphemeralVariables();
    }
    
    public void MarkVariableAsEphemeral(Variable variable)
    {
        EphemeralVariables.Add(variable);
    }

    protected void WipeEphemeralVariables()
    {
        if (EphemeralVariables.Count is 0) return;
        
        foreach (var variable in EphemeralVariables)
        {
            Script.RemoveLocalVariable(variable);
        }
        
        EphemeralVariables.Clear();
    }

    protected override void OnEndedExecution()
    {
        WipeEphemeralVariables();
    }
}