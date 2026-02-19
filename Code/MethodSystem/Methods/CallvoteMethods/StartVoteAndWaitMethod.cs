using Callvote.API;
using Callvote.API.VoteTemplate;
using Callvote.Features;
using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using MEC;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.MethodSystem.BaseMethods.Yielding;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.MethodSystem.Structures;
using SER.Code.ValueSystem;

// ReSharper disable LoopCanBeConvertedToQuery

/*
THE ISSUE
Our plugin supports optional integrations (e.g., Callvote-LabAPI). While we expect these features to simply "not run" 
when the dependency is missing, the LabAPI Plugin Loader can fail to load our entire assembly if the code is not 
structured correctly.

This happens because the C# compiler "lowers" modern features—like Async/Await, Yield Iterators, and LINQ/Lambdas—into 
hidden helper classes.

WHY STANDARD CODE FAILS
When you use a type from an external DLL (e.g., CustomVote) inside a lambda or an iterator, the compiler does the following:

1. State Machines: It turns your method into a private class.
2. Field Generation: It moves your local variables into FIELDS of that private class.
3. The Crash: When LabAPI scans our DLL, it looks at every class and every field. If it sees a field of a type it can't 
find (because the DLL is missing), it throws a ReflectionTypeLoadException and disables our plugin entirely.

BEST PRACTICES FOR SOFT DEPENDENCIES
To ensure the plugin loads even when dependencies are missing, follow these rules within integration-specific classes:

1. AVOID LINQ AND LAMBDAS
Do not use .Select(), .Where(), or any anonymous "=>" functions that touch external types.

* Problem: The compiler generates a "Display Class" with static fields to cache these delegates.
* Solution: Use standard "foreach" loops and manual logic.

2. ISOLATE ITERATORS AND ASYNC LOGIC
Never declare a variable of an external type directly inside a method that uses "yield return" or "async".

* Problem: The variable becomes a class field in the generated state machine.
* Solution: Move the specific logic into a "Safe" helper method (a standard "void" or non-iterator method).

3. USE "LATE-BOUND" LOGIC
Ensure that any code touching an external DLL is only called after a check (e.g., if PluginIsPresent). The .NET Runtime 
will only attempt to resolve the missing types at the moment that specific method is executed.

SUMMARY CHECKLIST
* Is this variable a Field in a class? (If yes, it must be a standard type).
* Am I using LINQ on external types? (If yes, replace with foreach).
* Is this an Iterator (yield) or Async method? (If yes, move external type logic to a separate helper method).

By following these constraints, we ensure that our assembly remains "scannable" by the loader, regardless of which 
optional plugins the user has installed.
 */
namespace SER.Code.MethodSystem.Methods.CallvoteMethods;

[UsedImplicitly]
public class StartVoteAndWaitMethod : YieldingReturningMethod<TextValue>, IAdditionalDescription, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.Callvote;
    
    public override string Description => "Starts a vote and waits until it is completed.";
    
    public string AdditionalDescription => 
        "It also returns the option key that won. If it was a tie, \"tie\" will be returned.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("question"),
        new PlayerArgument("player asking")
        {
            Description = "Use _ if there isnt a specific player asking the question.",
            DefaultValue = new(null, "general question")
        },
        new ReferenceArgument<VoteOptionMethod.VoteOption>("options")
        {
            Description = "The options for the vote.",
            ConsumesRemainingValues = true
        }
    ];

    public override IEnumerator<float> Execute()
    {
        var question = Args.GetText("question");
        var rawOptions = Args.GetRemainingArguments<
            VoteOptionMethod.VoteOption, 
            ReferenceArgument<VoteOptionMethod.VoteOption>>("options");
        
        bool completed = false;
        string result = "";

        // Run the logic in a SEPARATE method that is NOT an iterator.
        // This prevents 'CustomVote' from becoming a field in the <Execute> state machine.
        RunSafeVote(question, rawOptions, (res) => {
            result = res;
            completed = true;
        });
        
        yield return Timing.WaitUntilTrue(() => completed);

        ReturnValue = result.ToStaticTextValue();
    }

    private static void RunSafeVote(
        string question, 
        IEnumerable<VoteOptionMethod.VoteOption> rawOptions, 
        System.Action<string> onComplete)
    {
        var voteOptions = new HashSet<VoteOption>();
        foreach (var o in rawOptions)
        {
            voteOptions.Add(new VoteOption(o.Option, o.DisplayText));
        }

        var voting = new CustomVote(
            Server.Host!,
            question,
            $"SER.{question}",
            VoteCallback,
            voteOptions
        );
    
        VoteHandler.CallVote(voting);
        return;

        // Use a delegate pointing to a named method instead of a lambda
        // This keeps the logic isolated and avoids compiler-generated closures
        void VoteCallback(Vote vote)
        {
            int maxValue = 0;
            foreach (var val in vote.Counter.Values)
            {
                if (val > maxValue) maxValue = val;
            }

            List<string> topKeys = [];
            foreach (var kvp in vote.Counter)
            {
                if (kvp.Value == maxValue)
                {
                    // Accessing .Option here is safe because this method 
                    // is only JITted if Callvote is present.
                    topKeys.Add(kvp.Key.Option); 
                }
            }

            string finalResult = topKeys.Count > 1 ? "tie" : (topKeys.Count == 0 ? "none" : topKeys[0]);
            onComplete(finalResult);
        }
    }
}