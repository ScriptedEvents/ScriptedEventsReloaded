using Exiled.API.Enums;
using Exiled.API.Features;
using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.EffectMethods;

[UsedImplicitly]
public class ClearEffectMethod : SynchronousMethod, IDependOnFramework
{
    public override string Description => "Removes the provided status effect from players.";

    public override Argument[] ExpectedArguments =>
    [
        new PlayersArgument("players"),
        new EnumArgument<EffectType>("effect type")
            { DefaultValue = new (null, "Removes all status effects") },
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var effectType = Args.GetNullableEnum<EffectType>("effect type");
        
        if (effectType.HasValue)
            players.ForEach(plr => Player.Get(plr).DisableEffect(effectType.Value));
        else
            players.ForEach(plr => Player.Get(plr).DisableAllEffects());
    }

    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.Exiled;
}