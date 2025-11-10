using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.Helpers.Exceptions;
using SER.MethodSystem.BaseMethods;
using SER.MethodSystem.MethodDescriptors;

namespace SER.MethodSystem.Methods.AudioMethods;

public class LoadAudioMethod : SynchronousMethod, IAdditionalDescription, ICanError
{
    public override string Description => "Loads an audio file into the audio player.";

    public string AdditionalDescription =>
        "SER is using 'AudioPlayerApi' to manage audio. If the method errors, the logs will be displayed by AudioPlayerApi, not SER.";

    public string[] ErrorReasons =>
    [
        "File doesn't exist",
        "File was already loaded",
        "File is not of type 'ogg'"
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("file path"),
        new TextArgument("clip name")
        {
            Description = "You will be using this name to refer to this path."
        }
    ];

    public override void Execute()
    {
        if (!AudioClipStorage.LoadClip(
            Args.GetText("file path"), 
            Args.GetText("clip name")
        )) throw new ScriptRuntimeError("Audio has failed to load");
    }
}