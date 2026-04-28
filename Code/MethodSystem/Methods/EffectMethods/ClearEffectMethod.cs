using System.Reflection;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.EffectMethods;

[UsedImplicitly]
public class ClearEffectMethod : SynchronousMethod
{
    private static readonly MethodInfo DisableEffectMethod = 
        typeof(Player).GetMethod("DisableEffect", [])
        ?? throw new Exception("Could not find EnableEffect method for Player");
    
    public override string Description => "Removes the provided status effect from players.";

    public override Argument[] ExpectedArguments =>
    [
        new PlayersArgument("players"),
        new EffectTypeArgument("effect")
        {
            DefaultValue = new(null, "Removes all status effects")
        }
    ];

    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var effectType = Args.GetEffectType("effect").MaybeNull();

        var disableEffect = DisableEffectMethod.MakeGenericMethod(effectType);
        
        if (effectType is null)
        {
            foreach (var plr in players)
            {
                plr.DisableAllEffects();
            }
        }
        else
        {
            foreach (var plr in players)
            {
                disableEffect.Invoke(plr, null);
            }
        }
    }
}