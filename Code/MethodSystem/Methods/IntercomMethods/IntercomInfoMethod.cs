using LabApi.Features.Wrappers;
using PlayerRoles.Voice;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.ArgumentSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem.Other;

namespace SER.Code.MethodSystem.Methods.IntercomMethods;

[UsedImplicitly]
public class IntercomInfoMethod : ReturningMethod
{
    public override string Description => "Returns info about the Intercom.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new OptionsArgument("mode",
            Option.Enum<IntercomState>("state"),
            "speaker",
            "cooldown",
            "speechTimeLeft",
            "textOverride"
        )
    ];

    public override TypeOfValue Returns => new TypesOfValue([
        typeof(TextValue),
        typeof(PlayerValue),
        typeof(DurationValue)
    ]);
    
    public override void Execute()
    {
        ReturnValue = (Args.GetOption("mode")) switch
        {
            "state" => Intercom.State.ToEnumValue(),
            "speaker" => new PlayerValue(Player.ReadyList.ToList().Where(plr => plr.ReferenceHub == Intercom._singleton._curSpeaker)),
            "cooldown" => new DurationValue(TimeSpan.FromSeconds(Intercom.State == IntercomState.Cooldown ? Intercom._singleton.RemainingTime : 0)),
            "speechtimeleft" => new DurationValue(TimeSpan.FromSeconds(Intercom.State == IntercomState.InUse ? Intercom._singleton.RemainingTime : 0)),
            "textoverride" => new StaticTextValue(IntercomDisplay._singleton._overrideText),
            _ => throw new TosoksFuckedUpException("out of range")
        };
    }
}