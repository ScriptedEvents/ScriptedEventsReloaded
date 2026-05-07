using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;

namespace SER.Code.MethodSystem.Methods.AudioMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Audio_LoadMethod : SynchronousMethod, IAdditionalDescription, ICanError
{
    public override string Description => "Loads an audio file into the audio player.";

    public string AdditionalDescription =>
        """
        SER is using 'AudioPlayerApi' to manage audio. 
        This method does not error using SER's system, most audio errors will be logged by AudioPlayerApi to the console.
        Your .ogg file MUST BE:
        - 48kHz
        - single (mono) channel
        - medium quality
        """;

    public string[] ErrorReasons =>
    [
        "File doesn't exist",
        "File was already loaded",
        "File is not of type 'ogg'"
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("file path")
        {
            Description = 
                "This path starts at the main SER folder. " +
                "If your file is in [.. -> Scripted Events Reloaded -> audio.ogg] path, then the path will be 'audio.ogg'. " +
                "If your file is deeper, like [.. -> Scripted Events Reloaded -> subfolder -> audio.ogg], then the path will be 'subfolder/audio.ogg'." 
        },
        new TextArgument("clip name")
        {
            Description = "This will be the name of the audio clip. Refer to this name when attempting to play audio."
        }
    ];

    public override void Execute()
    {
        if (!AudioClipStorage.LoadClip(
            Path.Combine(FileSystem.FileSystem.MainDirPath, Args.GetText("file path")), 
            Args.GetText("clip name")
        )) throw new ScriptRuntimeError(this, "Audio has failed to load. Check the console for more info.");
    }
}