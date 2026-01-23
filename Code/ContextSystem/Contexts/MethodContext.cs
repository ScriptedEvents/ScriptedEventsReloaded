using MEC;
using SER.Code.ArgumentSystem;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.MethodSystem.BaseMethods.Interfaces;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.BaseMethods.Yielding;
using SER.Code.Plugin;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;
using MethodToken = SER.Code.TokenSystem.Tokens.MethodToken;
using ReturningMethod = SER.Code.MethodSystem.BaseMethods.Synchronous.ReturningMethod;

namespace SER.Code.ContextSystem.Contexts;

public class MethodContext(MethodToken methodToken) : YieldingContext, IMayReturnValueContext
{
    public readonly Method Method = methodToken.Method;
    public readonly MethodArgumentDispatcher Dispatcher = new(methodToken.Method);
    private int _providedArguments = 0;
    
    public TypeOfValue? Returns => Method is IReturningMethod returningMethod 
        ? returningMethod.Returns 
        : null;
    
    public Value? ReturnedValue { get; set; }

    public string MissingValueHint => "This method did not return a value. This may be a SER bug.";
    public string UndefinedReturnsHint => "This method does not define a return type. This may be a SER bug.";

    protected override string FriendlyName => $"'{Method.Name}' method call";

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

        if (MainPlugin.Instance.Config?.SafeScripts is true)
        {
            yield return Timing.WaitForOneFrame;
        }
        
        switch (Method)
        {
            case SynchronousMethod stdAct:
                stdAct.Execute();
                break;
            
            case YieldingMethod yieldAct:
                var enumerator = yieldAct.Execute();
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
                break;
        }

        ReturnedValue = Method is IReturningMethod returningMethod
            ? returningMethod.ReturnValue
            : null;
    }
}