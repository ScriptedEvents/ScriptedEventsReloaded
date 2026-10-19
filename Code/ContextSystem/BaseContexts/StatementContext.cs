using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.ContextSystem.BaseContexts;

public abstract class StatementContext : YieldingContext
{
    public readonly List<RunnableContext> Children = [];
    public readonly HashSet<Variable> EphemeralVariables = [];
    public uint? EndLine;

    // The body run that is executing right now. A function can call itself, so the same statement
    // can be in the middle of several runs at once. Each run keeps its own stop state, and only the
    // innermost (most recently started) run receives control messages.
    private BodyRun? _activeRun;

    /// <summary>
    ///     Tracks a single run of a statement's body.
    /// </summary>
    protected sealed class BodyRun
    {
        /// <summary>
        ///     The control message ('continue', 'break' or 'return') that stopped this run early, if any.
        /// </summary>
        public ParentContextControlMessage? StoppedBy { get; set; }
    }

    public void SendControlMessage(ParentContextControlMessage msg)
    {
        Log.Debug($"{this} has received control message: {msg}");

        // Every control message ends the body that is currently running, so its remaining lines must not run.
        if (_activeRun is { StoppedBy: null } run)
        {
            run.StoppedBy = msg;
        }

        OnReceivedControlMessageFromChild(msg);
    }

    protected virtual void OnReceivedControlMessageFromChild(ParentContextControlMessage msg)
    {
        ParentContext?.SendControlMessage(msg);
    }

    [MustDisposeResource]
    protected IEnumerator<float> RunChildren(Func<bool>? endCond = null)
    {
        return RunChildren(new BodyRun(), endCond);
    }

    /// <summary>
    ///     Runs the children, stopping early when one of them sends a control message to this statement.
    ///     Inspect <see cref="BodyRun.StoppedBy"/> afterward to see why the run stopped.
    /// </summary>
    [MustDisposeResource]
    protected IEnumerator<float> RunChildren(BodyRun run, Func<bool>? endCond = null)
    {
        // If we can run the whole thing synchronously right now, do it.
        // This still bypasses allocating OUR state machine.
        return RunChildrenInternal(run, endCond) ?? Enumerable.Empty<float>().GetEnumerator();
    }

    private static bool ShouldLeave(BodyRun run, Func<bool>? endCond)
    {
        return run.StoppedBy is not null || endCond?.Invoke() is true;
    }

    // The compiler only builds a state machine for this helper method
    private IEnumerator<float>? RunChildrenInternal(BodyRun run, Func<bool>? endCond)
    {
        var previousRun = _activeRun;
        _activeRun = run;
        var handedOff = false;
        try
        {
            for (int i = 0; i < Children.Count; i++)
            {
                if (ShouldLeave(run, endCond)) break;

                var child = Children[i];
                switch (child)
                {
                    case StandardContext sc:
                        sc.Run();
                        continue;

                    case YieldingContext:
                        // We hit a yielding context, so we MUST yield.
                        // We pass the remaining index so we know where to resume.
                        handedOff = true;
                        return RunChildrenYielding(i, run, endCond);
                }
            }

            return null;
        }
        finally
        {
            // The yielding runner makes the run active again when it starts, and cleans up when it ends.
            _activeRun = previousRun;
            if (!handedOff) WipeEphemeralVariables();
        }
    }

    // This handles the heavy lifting ONLY when a yield is actually encountered
    private IEnumerator<float> RunChildrenYielding(int startIndex, BodyRun run, Func<bool>? endCond)
    {
        var previousRun = _activeRun;
        _activeRun = run;
        try
        {
            for (int i = startIndex; i < Children.Count; i++)
            {
                if (ShouldLeave(run, endCond)) yield break;

                var child = Children[i];
                switch (child)
                {
                    case StandardContext sc:
                        sc.Run();
                        continue;

                    case YieldingContext yc:
                    {
                        using var coro = yc.Run();
                        while (coro.MoveNext())
                        {
                            if (ShouldLeave(run, endCond)) yield break;
                            yield return coro.Current;
                        }
                        break;
                    }
                }
            }
        }
        finally
        {
            // Runs on normal completion, on errors, and when the run is abandoned early.
            _activeRun = previousRun;
            WipeEphemeralVariables();
        }
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
