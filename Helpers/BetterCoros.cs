using System;
using System.Collections.Generic;
using MEC;
using SER.Helpers.Exceptions;
using SER.Helpers.Extensions;
using SER.ScriptSystem;

namespace SER.Helpers;

public static class BetterCoros
{
    public static CoroutineHandle Run(this IEnumerator<float> coro, Script scr, Action<Exception>? onException = null)
    {
        return Timing.RunCoroutine(Wrapper(coro, scr, onException));
    }

    public static void Kill(this CoroutineHandle coro)
    {
        Timing.KillCoroutines(coro);
    }

    private static IEnumerator<float> Wrapper(IEnumerator<float> routine, Script scr, Action<Exception>? onException = null)
    {
        while (true)
        {
            try
            {
                if (!routine.MoveNext()) break;
            }
            catch (ScriptRuntimeError scrErr)
            {
                onException?.Invoke(scrErr);
                scr.Error(scrErr.Message!);
                yield break;
            }
            catch (DeveloperFuckedUpException devErr)
            {
                onException?.Invoke(devErr);
                scr.Error(devErr.Message + "\n" + devErr.StackTrace);
                yield break;
            }
            catch (Exception ex)
            {
                onException?.Invoke(ex);
                scr.Error($"Coroutine failed with {ex.GetType().AccurateName}: {ex.Message}\n{ex.StackTrace}");
                yield break;
            }

            yield return routine.Current;
        }
    }
}