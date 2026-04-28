using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;

namespace SER.Code.MethodSystem.Methods.AudioMethods;

[UsedImplicitly]
public class PlayAudioMethod : SynchronousMethod, ICanError
{
    public override string Description => "Plays a loaded audio clip from a created speaker.";

    public string[] ErrorReasons =>
    [
        "Speaker does not exist.",
        "There is no loaded audio clip with the given name."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("speaker name"),
        new TextArgument("audio clip name"),
        new BoolArgument("loop?")
        {
            DefaultValue = new(false, null)
        }
    ];

    public override void Execute()
    {
        var speakerName = Args.GetText("speaker name");
        var clipName = Args.GetText("audio clip name");
        
        if (!AudioPlayer.TryGet(speakerName, out AudioPlayer audioPlayer))
            throw new ScriptRuntimeError(this, $"Speaker with name '{speakerName}' does not exist.");

        if (!AudioClipStorage.AudioClips.ContainsKey(clipName))
            throw new ScriptRuntimeError(this, $"Audio clip with name '{clipName}' does not exist.");
        
        audioPlayer.AddClip(clipName, loop: Args.GetBool("loop?"));
    }
}