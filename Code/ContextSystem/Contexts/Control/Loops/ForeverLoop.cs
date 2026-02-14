using JetBrains.Annotations;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.ResultSystem;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.MethodSystem.Methods.WaitingMethods;
using SER.Code.Plugin;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;

namespace SER.Code.ContextSystem.Contexts.Control.Loops;

[UsedImplicitly]
public class ForeverLoop : LoopContextWithSingleIterationVariable<NumberValue>, IKeywordContext
{
    public override string KeywordName => "forever";
    public override string Description => "Makes the code inside the statement run indefinitely.";
    public override string[] Arguments => [];

    protected override string Usage =>
        $$"""
        # {{Description}}
        # it can be interrupted only when the script is stopped, when "break" keyword is used, or the server restarts
        # it's VERY IMPORTANT to use yielding methods like "{{Method.GetFriendlyName(typeof(WaitMethod))}}"
        #  or else YOUR SERVER MAY CRASH!!!
        
        # this will send an ad every 2 minutes
        forever
            Wait 2m
            Broadcast * 10s "Join our discord server! {{MainPlugin.DiscordLink}}"
        end
        
        # ========================================
        # you can also use "with" keyword to define an interation variable
        #  which will hold the current iteration number, starting from 1
        forever
            with $iter
            
            Wait 1s
            Print "current iteration: {$iter}
        end
        """;
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    private readonly Result _mainErr = "Cannot create 'forever' loop.";
    public override Dictionary<IExtendableStatement.Signal, Func<IEnumerator<float>>> RegisteredSignals { get; } =
        new();

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        return TryAddTokenRes.Error(_mainErr + "'forever' loop doesn't expect any arguments.");
    }

    public override Result VerifyCurrentState()
    {
        return true;
    }

    protected override IEnumerator<float> Execute()
    {
        ulong iteration = 0;
        while (true)
        {
            SetVariable(++iteration);
            var coro = RunChildren();
            while (coro.MoveNext())
            {
                yield return coro.Current;
            }
            RemoveVariable();
            
            if (ReceivedBreak) break;
        }
    }
}