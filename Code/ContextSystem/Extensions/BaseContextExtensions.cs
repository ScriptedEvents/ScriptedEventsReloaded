using SER.Code.ContextSystem.BaseContexts;
using SER.Code.Helpers;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.Extensions;

namespace SER.Code.ContextSystem.Extensions;

public static class BaseContextExtensions
{
    public static IEnumerator<float> ExecuteBaseContext(this Context context)
    {
        Log.Debug($"Executing context {context.FriendlyTypeName()}");
        switch (context)
        {
            case StandardContext standardContext:
                standardContext.Run();
                yield break;
            
            case YieldingContext yieldingContext:
                var coro = yieldingContext.Run();
                while (coro.MoveNext())
                {
                    if (!context.Script.IsRunning)
                    {
                        yield break;
                    }
                    
                    yield return coro.Current;
                }
                
                yield break;
            
            default:
                throw new AndrzejFuckedUpException("context is not standard nor yielding");
        }
    }
}