using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.Helpers.Exceptions;
using SER.MethodSystem.BaseMethods;
using SER.MethodSystem.MethodDescriptors;

namespace SER.MethodSystem.Methods.AudioMethods;

public class PlayAudioMethod : SynchronousMethod, ICanError
{
    public override string Description => "Plays a loaded audio clip from a created speaker.";

    public string[] ErrorReasons =>
    [
        "Speaker does not exist."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("speaker name"),
        new TextArgument("clip name"),
        new BoolArgument("loop music")
        {
            DefaultValue = new(false, null)
        }
    ];

    public override void Execute()
    {
        var speakerName = Args.GetText("speaker name");
        var clipName = Args.GetText("clip name");
        
        if (!AudioPlayer.TryGet(speakerName, out AudioPlayer audioPlayer))
            throw new ScriptRuntimeError($"Speaker with name '{speakerName}' does not exist.");
        
        audioPlayer.AddClip(clipName, loop: Args.GetBool("loop music"));
    }
}