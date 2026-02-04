using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.VotingMethods;

[UsedImplicitly]
public class VoteOptionMethod : ReferenceReturningMethod<VoteOptionMethod.VoteOption>, IDependOnFramework
{
    public IDependOnFramework.Type DependsOn => IDependOnFramework.Type.Callvote;

    public record VoteOption(string Key, string DisplayText);

    public override string Description => "Creates a vote option, which can be used in a vote.";
    
    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("key", allowsSpaces: false)
        {
            Description = "This will be the command for voting AND the result of the vote, if it wins."
        },
        new TextArgument("display text")
        {
            Description = "The description of the option."
        }
    ];

    public override void Execute()
    {
        var key = Args.GetText("key");
        var displayText = Args.GetText("display text");

        ReturnValue = new(key, displayText);
    }
}