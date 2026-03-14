using JetBrains.Annotations;
using PlayerRoles.FirstPersonControl;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class StaminaMethod : SynchronousMethod
{
    public override string Description => "Control the stamina of players.";

    public override Argument[] ExpectedArguments =>
    [
        new OptionsArgument("options", options:
        [
            new("add"),
            new("remove"),
            new("set"),
        ]),
        new PlayersArgument("players"),
        new FloatArgument("stamina value", 0f, 1f)
        {
            Description = "Stamina is valued from 0 to 1. 0 meaning an empty stamina bar and 1 meaning a full stamina bar."
        },
        new BoolArgument("delay stamina regen")
        {
            Description = "Stops stamina regeneration for a short duration after applying new stamina value, just like at the end of a sprint.",
            DefaultValue = new(true, "will delay stamina regeneration for a second when new stamina value is applied.")
        }
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var staminaValue = Args.GetFloat("stamina value");

        players.ForEach(plr =>
        {
            if (plr?.RoleBase is not IFpcRole currentRole) return;
            var newStamina = 0f;
            switch (Args.GetOption("options"))
            {
                case "add":
                    newStamina = plr.StaminaRemaining + staminaValue;
                    if (newStamina > 1f) newStamina = 1f;
                    break;
                case "remove":
                    newStamina = plr.StaminaRemaining - staminaValue;
                    if (newStamina < 0f) newStamina = 0f;
                    break;
                case "set":
                    newStamina = staminaValue;
                    break;
            }

            plr.StaminaRemaining = newStamina;
            if (Args.GetBool("delay stamina regen"))
                currentRole.FpcModule.StateProcessor._regenStopwatch.Restart();
        });
    }
}