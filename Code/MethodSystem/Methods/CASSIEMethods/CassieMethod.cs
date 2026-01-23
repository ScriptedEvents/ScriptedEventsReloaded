using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

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
        new FloatArgument("glitch scale", 0, 1)
        {
            DefaultValue = new(0f, "0%"),
            Description = "The amount of glitching to apply to the announcement, from 0% to 100%"
        }
    ];
    
    public override void Execute()
    {
        var isNoisy = Args.GetOption("mode") == "jingle";
        var message = Args.GetText("message");
        var subtitles = Args.GetText("subtitles");
        var glitch = Args.GetFloat("glitch scale");
        
        Announcer.Message(
            message, 
            subtitles,
            isNoisy,
            glitchScale: glitch
        );
    }
}