using LabApi.Features.Console;
using MEC;
using SER.Code.Exceptions;
using SER.Code.Plugin;
using SER.Code.ScriptSystem;
using SER.Code.ScriptSystem.Structures;

namespace SER.Code.Helpers;

public static class BetterCoros
{
    public static CoroutineHandle Run(
        this IEnumerator<float> coro,
        Script? scr,
        Action<Exception>? onException = null,
        Action? onFinish = null
    )
    {
        return Timing.RunCoroutine(Wrapper(coro, scr, onException, onFinish));
    }

    public static void Kill(this CoroutineHandle coro)
    {
        Timing.KillCoroutines(coro);
    }

    private static IEnumerator<float> Wrapper(
        IEnumerator<float> routine,
        Script? scr,
        Action<Exception>? onException = null,
        Action? onFinish = null
    )
    {
        while (true)
        {
            if (MainPlugin.Instance.Config.SafeScripts)
            {
                yield return Timing.WaitForOneFrame;
            }

            try
            {
                if (!routine.MoveNext()) goto End;
            }
            catch (StopScript)
            {
                goto End;
            }
            catch (ScriptCompileError compErr)
            {
                onException?.Invoke(compErr);
                scr?.Error(compErr.Message);
                goto End;
            }
            catch (ScriptRuntimeError runErr)
            {
                onException?.Invoke(runErr);
                scr?.Error(runErr.Message);
                goto End;
            }
            catch (DeveloperFuckedUpException devErr)
            {
                ReportInternalError(devErr, scr, onException);
                goto End;
            }
            catch (Exception ex)
            {
                ReportInternalError(ex, scr, onException);
                goto End;
            }

            if (scr?.Killed is true)
            {
                goto End;
            }

            yield return routine.Current;
        }

        End:
        onFinish?.Invoke();
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
