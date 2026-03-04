using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.AudioMethods;

[UsedImplicitly]
public class IsPlayingAudioMethod : ReturningMethod<BoolValue>, ICanError
{
    public override string Description => "Checks if the audio player is playing anything.";

    public string[] ErrorReasons =>
    [
        "Speaker doesn't exist."
    ];
    
    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("speaker name")
    ];
    
    public override void Execute()
    {
        var speakerName = Args.GetText("speaker name");
        if (!AudioPlayer.TryGet(speakerName, out var speaker))
            throw new ScriptRuntimeError(this, $"Speaker '{speakerName}' doesn't exist.");

        ReturnValue = !speaker.ClipsById.IsEmpty();
    }

}
