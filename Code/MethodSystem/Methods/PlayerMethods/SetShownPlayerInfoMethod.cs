using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class SetShownPlayerInfoMethod : SynchronousMethod
{
    public override string Description => "Sets what information about the player is shown.";

    public override Argument[] ExpectedArguments { get; } = 
    [
        new PlayersArgument("players"),
        new EnumArgument<PlayerInfoArea>("info to show")
        {
            ConsumesRemainingValues = true,
        }
    ];

    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        PlayerInfoArea info = Args.GetRemainingArguments<PlayerInfoArea, EnumArgument<PlayerInfoArea>>("info area")
            .Aggregate((a, b) => a | b);
        
        players.ForEach(p => p.InfoArea = info);
    }
}
