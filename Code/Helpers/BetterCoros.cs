using MEC;
using SER.Code.Exceptions;
using SER.Code.Plugin;
using SER.Code.ScriptSystem;
using SER.Code.ScriptSystem.Structures;

namespace SER.Code.Helpers;

public static class BetterCoros
{
    private static readonly HashSet<CoroutineHandle> ActiveCoroutines = [];

    public static CoroutineHandle Run(
        this IEnumerator<float> coro,
        Script? scr,
        Action<Exception>? onException = null,
        Action? onFinish = null)
    {
        CoroutineHandle handle = default;
        var completedBeforeRegistration = false;
        handle = Timing.RunCoroutine(Wrapper(coro, scr, onException, () =>
        {
            completedBeforeRegistration = true;
            ActiveCoroutines.Remove(handle);
            onFinish?.Invoke();
        }));

        // MEC may complete a coroutine before RunCoroutine returns. Do not add a
        // handle which has already finished; later plugin cleanup would retain it.
        if (!completedBeforeRegistration)
        {
            ActiveCoroutines.Add(handle);
        }

        return handle;
    }

    public static void Kill(this CoroutineHandle coro)
    {
        Timing.KillCoroutines(coro);
    }

    public static void KillAll()
    {
        foreach (var coroutine in ActiveCoroutines.ToArray())
        {
            coroutine.Kill();
        }

        ActiveCoroutines.Clear();
    }

    private static IEnumerator<float> Wrapper(
        IEnumerator<float> routine,
        Script? scr,
        Action<Exception>? onException = null,
        Action? onFinish = null)
    {
        try
        {
            while (true)
            {
                if (scr?.Killed is true)
                {
                    yield break;
                }

                if (MainPlugin.Instance.Config.SafeScripts)
                {
                    yield return Timing.WaitForOneFrame;
                    if (scr?.Killed is true)
                    {
                        yield break;
                    }
                }

                try
                {
                    if (!routine.MoveNext()) yield break;
                }
                catch (StopScript)
                {
                    yield break;
                }
                catch (ScriptCompileError compErr)
                {
                    onException?.Invoke(compErr);
                    scr?.Error(compErr.Message);
                    yield break;
                }
                catch (ScriptRuntimeError runErr)
                {
                    onException?.Invoke(runErr);
                    scr?.Error(runErr.Message);
                    yield break;
                }
                catch (InternalSerException devErr)
                {
                    ReportInternalError(devErr, scr, onException);
                    yield break;
                }
                catch (Exception ex)
                {
                    ReportInternalError(ex, scr, onException);
                    yield break;
                }

                if (scr?.Killed is true)
                {
                    yield break;
                }

                yield return routine.Current;
            }
        }
        finally
        {
            try
            {
                routine.Dispose();
            }
            catch (Exception exception)
            {
                ReportInternalError(exception, scr, onException);
            }
            finally
            {
                onFinish?.Invoke();
            }
        }
    }

    private static void ReportInternalError(
        Exception exception,
        Script? script,
        Action<Exception>? onException)
    {
        var errorId = Guid.NewGuid().ToString("N")[..8];
        var scriptTrace = BuildScriptTrace(script);
        Log.Error($"SER internal error [{errorId}]{scriptTrace}\n{exception}");

        var publicError = new CustomScriptRuntimeError(
            $"Internal SER error [{errorId}]. Check the server console and report this identifier.");
        onException?.Invoke(publicError);
        script?.Error(publicError.Message);
    }

    private static string BuildScriptTrace(Script? script)
    {
        if (script is null)
        {
            return string.Empty;
        }

        var entries = new List<string>();
        var seen = new HashSet<Script>();
        for (var current = script; current is not null && seen.Add(current); current = current.Caller)
        {
            var location = current.CurrentLine == 0 ? "during setup" : $"line {current.CurrentLine}";
            entries.Add($"'{current.Name}' ({location}, started by {current.RunReason})");
        }

        return $"\nScript trace: {string.Join(" <- ", entries)}";
    }
}
