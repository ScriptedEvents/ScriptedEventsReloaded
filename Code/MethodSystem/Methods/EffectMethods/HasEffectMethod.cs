using Exiled.API.Enums;
using Exiled.API.Features;
using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.EffectMethods;

[UsedImplicitly]
public class HasEffectMethod : LiteralValueReturningMethod, IDependOnFramework
{
    public override TypeOfValue LiteralReturnTypes => new SingleTypeOfValue(typeof(BoolValue));
    
    public override string Description => "Returns true or false indicating if the player has the provided effect.";

    public override Argument[] ExpectedArguments =>
    [
        new PlayerArgument("player"),
        new EnumArgument<EffectType>("effect type")
    ];
    
    public override void Execute()
    {
        var player = Args.GetPlayer("player");
        var effectType = Args.GetEnum<EffectType>("effect type");

        if (!Player.Get(player).TryGetEffect(effectType, out var effect))
            ReturnValue = new BoolValue(false);
        
        // this feels kinda stupid, but you never know...
        ReturnValue = new BoolValue(effect.IsEnabled);
    }

    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.Exiled;
}