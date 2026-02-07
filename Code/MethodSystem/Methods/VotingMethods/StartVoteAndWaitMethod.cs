using Callvote.API;
using Callvote.API.VotingsTemplate;
using Callvote.Features;
using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using MEC;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.Interfaces;
using SER.Code.MethodSystem.BaseMethods.Yielding;
using SER.Code.MethodSystem.Structures;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.VotingMethods;

[UsedImplicitly]
public class StartVoteAndWaitMethod : YieldingReturningMethod<TextValue>, IAdditionalDescription, IDependOnFramework
{
    public IDependOnFramework.Type DependsOn => IDependOnFramework.Type.Callvote;
    
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
        var completed = false;
        string result = "";
        var question = Args.GetText("question");
        var options = Args.GetRemainingArguments<
            VoteOptionMethod.VoteOption, 
            ReferenceArgument<VoteOptionMethod.VoteOption>>("options");

        var voting = new CustomVoting(
            Server.Host!, 
            question,
            $"SER.{question}",
            Callback, 
            options.ToDictionary(o => o.Key, o => o.DisplayText)
        );
        
        VotingHandler.CallVoting(voting);

        yield return Timing.WaitUntilTrue(() => completed);

        ReturnValue = result.ToStaticTextValue();
        yield break;

        void Callback(Voting vote)
        {
            completed = true;

            int maxValue = vote.Counter.Values.Max();
            var topKeys = vote.Counter
                .Where(kvp => kvp.Value == maxValue)
                .Select(kvp => kvp.Key)
                .ToArray();

            result = topKeys.Length > 1 ? "tie" : topKeys[0];
        }
    }
}