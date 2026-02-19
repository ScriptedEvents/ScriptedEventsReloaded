using Callvote.API;
using Callvote.API.VoteTemplate;
using Callvote.Features;
using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.CallvoteMethods;

[UsedImplicitly]
public class StartVoteMethod : SynchronousMethod, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.Callvote;
    
    public override string Description => "Starts a vote.";

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

    public override void Execute()
    {
        var question = Args.GetText("question");
        var rawOptions = Args.GetRemainingArguments<
            VoteOptionMethod.VoteOption, 
            ReferenceArgument<VoteOptionMethod.VoteOption>>("options");
        
        var voteOptions = new HashSet<VoteOption>();
        foreach (var o in rawOptions)
        {
            voteOptions.Add(new VoteOption(o.Option, o.DisplayText));
        }

        var voting = new CustomVote(
            Server.Host!,
            question,
            $"SER.{question}",
            null,
            voteOptions
        );
        
        VoteHandler.CallVote(voting);
    }
}