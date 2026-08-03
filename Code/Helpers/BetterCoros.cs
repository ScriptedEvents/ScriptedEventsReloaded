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
        var handle1 = handle;
        handle = Timing.RunCoroutine(Wrapper(coro, scr, onException, () =>
        {
            ActiveCoroutines.Remove(handle1);
            onFinish?.Invoke();
        }));
        ActiveCoroutines.Add(handle);
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
        Log.Error($"SER internal error [{errorId}]\n{exception}");

        var publicError = new CustomScriptRuntimeError(
            $"Internal SER error [{errorId}]. Check the server console and report this identifier.");
        onException?.Invoke(publicError);
        script?.Error(publicError.Message);
    }
}
