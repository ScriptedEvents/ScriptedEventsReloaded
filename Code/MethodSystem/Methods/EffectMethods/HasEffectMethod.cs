using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.EffectMethods;

[UsedImplicitly]
public class HasEffectMethod : ReturningMethod<BoolValue>
{
    public override string Description => "Returns true or false indicating if the player has the provided effect.";

    public override Argument[] ExpectedArguments =>
    [
        new PlayerArgument("player"),
        new EffectTypeArgument("effect")
    ];
    
    public override void Execute()
    {
        var player = Args.GetPlayer("player");
        var effectType = Args.GetEffectType("effect");
        
        ReturnValue = player.ActiveEffects.Any(x => x.GetType() == effectType);
    }
}