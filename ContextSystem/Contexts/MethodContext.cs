using SER.ArgumentSystem;
using SER.ContextSystem.BaseContexts;
using SER.ContextSystem.Structures;
using SER.Helpers;
using SER.Helpers.Extensions;
using SER.Helpers.ResultSystem;
using SER.MethodSystem.BaseMethods;
using SER.TokenSystem.Tokens;
using MethodToken = SER.TokenSystem.Tokens.MethodToken;

namespace SER.ContextSystem.Contexts;

public class MethodContext(MethodToken methodToken) : YieldingContext
{
    public readonly Method Method = methodToken.Method;
    public readonly MethodArgumentDispatcher Dispatcher = new(methodToken.Method);
    private int _providedArguments = 0;
    
    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        Log.Debug($"'{Method.Name}' method is now receiving token '{token.RawRep}' ({token.GetType().AccurateName})");
        
        if (Dispatcher.TryGetValueInfo(token, _providedArguments).HasErrored(out var error, out var skeleton))
            return TryAddTokenRes.Error(
                $"Value '{token.RawRep}' is not a valid argument: " +
                $"{error}");
        
        Log.Debug($"skeleton {skeleton.Name} {skeleton.ArgumentType} registered");
        
        Method.Args.Add(skeleton);
        _providedArguments++;
        return TryAddTokenRes.Continue();
    }

    public override Result VerifyCurrentState()
    {
        return Result.Assert(_providedArguments >= Method.ExpectedArguments.Count(arg => arg.DefaultValue is null),
            $"Method '{Method.Name}' is missing required arguments: " +
            $"{", ".Join(Method.ExpectedArguments.Skip(_providedArguments).Select(arg => arg.Name))}");
    }

    protected override IEnumerator<float> Execute()
    {
        Log.Debug($"'{Method.Name}' method is now running..");

        switch (Method)
        {
            case SynchronousMethod stdAct:
                stdAct.Execute();
                yield break;
            case YieldingMethod yieldAct:
                var enumerator = yieldAct.Execute();
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
                yield break;
        }
    }
}