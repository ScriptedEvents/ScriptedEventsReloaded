using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
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
