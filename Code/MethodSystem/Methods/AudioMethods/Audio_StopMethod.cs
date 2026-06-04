using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.AudioMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Audio_StopMethod : SynchronousMethod, ICanError
{
    public override string Description => "Plays a loaded audio clip from a created speaker.";

    public string[] ErrorReasons =>
    [
        "Speaker does not exist."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("speaker name")
    ];

    public override void Execute()
    {
        var speakerName = Args.GetText("speaker name");
        
        if (!AudioPlayer.TryGet(speakerName, out AudioPlayer audioPlayer))
            throw new ScriptRuntimeError(this, $"Speaker with name '{speakerName}' does not exist.");
        
        audioPlayer.RemoveAllClips();
    }
}