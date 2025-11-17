using SER.MethodSystem.BaseMethods;
using SER.ArgumentSystem.BaseArguments;
using SER.ArgumentSystem.Arguments;
using SER.ValueSystem;
using PlayerRoles.Voice;
using LabApi.Features.Wrappers;
using SER.Helpers.Exceptions;

namespace SER.MethodSystem.Methods.IntercomMethods;

public class IntercomInfoMethod : ReturningMethod
{
    public override string? Description => "Returns info about the Intercom.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new OptionsArgument("mode",
            "state",
            "speaker",
            "cooldown",
            "speechTimeLeft")
    ];

    public override Type[] ReturnTypes => [typeof(TextValue), typeof(PlayerValue), typeof(DurationValue)];

    public override void Execute()
    {
        ReturnValue = (Args.GetOption("mode")) switch
        {
            "state" => new TextValue(Intercom.State.ToString()),
            "speaker" => new PlayerValue(Player.ReadyList.ToList().Where(plr => plr.ReferenceHub == Intercom._singleton._curSpeaker)),
            "cooldown" => new DurationValue(TimeSpan.FromSeconds(Intercom.State == IntercomState.Cooldown ? Intercom._singleton.RemainingTime : 0)),
            "speechtimeleft" => new DurationValue(TimeSpan.FromSeconds(Intercom.State == IntercomState.InUse ? Intercom._singleton.RemainingTime : 0)),
            _ => throw new TosoksFuckedUpException("out of range")
        };
    }
}