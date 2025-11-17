using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;

namespace SER.MethodSystem.Methods.PlayerMethods;

public class SetInfoAreaMethod : SynchronousMethod
{
    public override string Description => "Sets InfoArea for specified players";

    public override Argument[] ExpectedArguments { get; } = 
    [
        new PlayersArgument("players"),
        new EnumArgument<PlayerInfoArea>("info area")
        {
            ConsumesRemainingValues = true,
        }
    ];

    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        PlayerInfoArea info = Args.GetRemainingArguments<object, EnumArgument<PlayerInfoArea>>("info area")
            .Cast<PlayerInfoArea>()
            .Aggregate(PlayerInfoArea.Nickname, (a, b) => a | b);
        
        players.ForEach(p => p.InfoArea = info);
    }
}
