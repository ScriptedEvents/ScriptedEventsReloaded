using SER.Code.ContextSystem.Interfaces;
using SER.Code.Helpers;

namespace SER.Code.ContextSystem.BaseContexts;

public abstract class YieldingContext : RunnableContext
{
    public IEnumerator<float> Run()
    {
        if (this is INotRunningContext)
        {
            yield break;
        }

        var prof = Script.Profile is not null
            ? new Profile(Script.Profile, $"running YieldingContext {this}")
            : null;

        if (LineNum.HasValue)
        {
            Script.CurrentLine = LineNum.Value;
        }

        try
        {
            using var enumerator = Execute();
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }
        finally
        {
            // Also runs when the statement fails with an error or is abandoned early (for example by 'return'),
            // so cleanup such as removing ephemeral variables always happens.
            prof?.Stop();
            OnEndedExecution();
        }
    }

    protected abstract IEnumerator<float> Execute();

    protected virtual void OnEndedExecution()
    {
    }
}
