using Callvote.API;
using Callvote.API.VoteTemplate;
using Callvote.Features;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.CallvoteMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Vote_StartMethod : SynchronousMethod, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.Callvote;
    
    public override string Description => "Starts a vote.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("question"),
        new PlayerArgument("player asking")
        {
            Description = "Use _ if there isn't a specific player asking the question.",
            DefaultValue = new(null, "general question")
        },
        new ReferenceArgument<Vote_CreateOptionMethod.VoteOption>("options")
        {
            Description = "The options for the vote.",
            ConsumesRemainingValues = true
        }
    ];

    public override void Execute()
    {
        var question = Args.GetText("question");
        var askingPlayer = Args.GetPlayer("player asking").MaybeNull() ?? Server.Host!;
        var rawOptions = Args.GetRemainingArguments<
            Vote_CreateOptionMethod.VoteOption, 
            ReferenceArgument<Vote_CreateOptionMethod.VoteOption>>("options");
        
        var voteOptions = new HashSet<VoteOption>();
        foreach (var o in rawOptions)
        {
            voteOptions.Add(new VoteOption(o.Option, o.DisplayText));
        }

        var voting = new CustomVote(
            askingPlayer,
            question,
            $"SER.{question}",
            null,
            voteOptions
        );
        
        VoteHandler.CallVote(voting);
    }
}
