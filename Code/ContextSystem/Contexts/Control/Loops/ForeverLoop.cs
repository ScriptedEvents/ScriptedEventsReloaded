using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.ResultSystem;
using SER.Code.Plugin;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;

namespace SER.Code.ContextSystem.Contexts.Control.Loops;

[UsedImplicitly]
public class ForeverLoop : LoopContextWithSingleIterationVariable<NumberValue>, IKeywordContext
{
    private readonly Result _mainErr = "Cannot create 'forever' loop.";

    protected override string Usage =>
        $$"""
          # {{Description}}
          # it can be interrupted only when the script is stopped, when "break" keyword is used, or the server restarts
          # it's VERY IMPORTANT to use yielding methods like "wait"
          #  or else YOUR SERVER MAY CRASH!!!

          # this will send an ad every 2 minutes
          forever
              wait 2m
              Broadcast @all 10s "Join our discord server! {{MainPlugin.DiscordLink}}"
          end

          # ========================================
          # you can also use "with" keyword to define an iteration variable
          #  which will hold the current iteration number, starting from 1
          forever with $iter
              wait 1s
              Print "current iteration: {$iter}"
          end
          """;
    public override string KeywordName => "forever";
    public override string Description => "Makes the code inside the statement run indefinitely.";
    public override string[] Arguments => [];

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