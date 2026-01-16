using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;

namespace SER.Code.MethodSystem.Methods.CASSIEMethods;

[UsedImplicitly]
public class CassieMethod : SynchronousMethod
{
    public override string Description => "Makes a CASSIE announcement.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new OptionsArgument("mode",
            "jingle",
            "noJingle"
        ),
        new TextArgument("message"),
        new TextArgument("subtitles")
        {
            DefaultValue = new("", "empty"),
        },
        new BoolArgument("should glitch")
        {
            Description = "If true, SER will add random glitch effects to the announcement.",
            DefaultValue = new(false, null),
        }
    ];
    
    public override void Execute()
    {
        var isNoisy = Args.GetOption("mode") == "jingle";
        var message = Args.GetText("message");
        var subtitles = Args.GetText("subtitles");
        var glitch = Args.GetBool("should glitch");

        // todo: check if this is still needed
        if (glitch)
        {
            // taken from Respawning.Announcements.WaveAnnouncementBase.PlayAnnouncement()
            var chanceMultiplier = AlphaWarheadController.Detonated ? 2.5f : 1;
            var glitchChance = UnityEngine.Random.Range(0.08f, 0.1f) * chanceMultiplier;
            var jamChance = UnityEngine.Random.Range(0.07f, 0.09f) * chanceMultiplier;
            
            var strArray = message.Split([' '], StringSplitOptions.None);
            message = "";
            // taken from NineTailedFoxAnnouncer.ServerOnlyAddGlitchyPhrase()
            for (var index = 0; index < strArray.Length; index++)
            {
                message += $"{strArray[index]} ";
                
                if (index >= strArray.Length - 1)
                {
                    continue;
                }
                
                if (UnityEngine.Random.value < glitchChance)
                {
                    message += $".G{UnityEngine.Random.Range(1, 7)} ";
                }
                
                if (UnityEngine.Random.value < jamChance)
                {
                    message += $"jam_{UnityEngine.Random.Range(0, 70):000}_{UnityEngine.Random.Range(2, 6)} ";
                }
            }
        }
        
        // todo: check if this glitchScale works as intended
        Announcer.Message(
            message, 
            translation,
            glitchScale: isNoisy ? 1 : 0
        );
    }
}