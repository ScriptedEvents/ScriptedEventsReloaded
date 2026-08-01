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

        using var enumerator = Execute();
        while (enumerator.MoveNext())
        {
            yield return enumerator.Current;
        }

        prof?.Stop();
        OnEndedExecution();
    }

    protected abstract IEnumerator<float> Execute();

    protected virtual void OnEndedExecution()
    {
    }
}
