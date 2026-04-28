using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.FlagSystem.Flags;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.CommandMethods;

[UsedImplicitly]
public class ResetPlayerCommandCooldownMethod : SynchronousMethod
{
    public override string Description => "Resets a player's command cooldown from the 'CustomCommand' flag.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<CustomCommandFlag.CustomCommand>("command"),
        new PlayersArgument("players")
        {
            DefaultValue = new(null, "all players")
        }
    ];
    
    public override void Execute()
    {
        var dict = Args
            .GetReference<CustomCommandFlag.CustomCommand>("command")
            .NextEligibleDateForPlayer;

        if (Args.GetPlayers("players") is { } players)
        {
            foreach (var player in players)
            {
                dict.Remove(player);
            }
        }
        else
        {
            dict.Clear();
        }
    }
}