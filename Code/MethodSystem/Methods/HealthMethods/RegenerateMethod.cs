using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.HealthMethods;

[UsedImplicitly]
public class RegenerateMethod : SynchronousMethod
{
    public override string Description => "Adds health regeneration to players.";

    public override Argument[] ExpectedArguments =>
    [
        new PlayersArgument("players"),
        new FloatArgument("regeneration rate"),
        new FloatArgument("regeneration duration")
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var regenerationRate = Args.GetFloat("regeneration rate");
        var regenerationDuration = Args.GetFloat("regeneration duration");
        players.ForEach(plr => plr.AddRegeneration(regenerationRate, regenerationDuration));
    }
}