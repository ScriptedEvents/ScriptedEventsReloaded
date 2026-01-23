using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;
using Utils;

namespace SER.Code.MethodSystem.Methods.ParsingMethods;

[UsedImplicitly]
public class ParsePlayersMethod : ReturningMethod<PlayerValue>, IAdditionalDescription
{
    public override string Description => "Parses a string containing player IDs into a PlayerValue";

    public string AdditionalDescription =>
        "It functions the same as most RA commands do e.g. \"1.3.5.6\" would return a PlayerValue " +
        "containing players with the player IDs of 1,3,5 and 6.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("player IDs")
    ];

    public override void Execute()
    {
        var playersString = Args.GetText("player IDs");
        var playerHubs = RAUtils.ProcessPlayerIdOrNamesList(
            new ArraySegment<string>([playersString]),
            0,
            out _
            );
        ReturnValue = new PlayerValue(Player.Get(playerHubs));
    }
}