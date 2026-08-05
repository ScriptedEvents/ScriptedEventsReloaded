using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;

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
        "File is not of type 'ogg'",
        "More than one file with the same name exists"
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("file name")
        {
            Description = "The .ogg file name. SER searches all folders under its main data directory automatically."
        },
        new TextArgument("clip name")
        {
            Description = "This will be the name of the audio clip. Refer to this name when attempting to play audio."
        }
    ];

    public override void Execute()
    {
        var name = Args.GetText("clip name");
        if (AudioClipStorage.AudioClips.ContainsKey(name))
        {
            return;
        }
        
        var fileName = Args.GetText("file name");
        if (!fileName.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
            throw new ScriptRuntimeError(this, "Audio file name must have an '.ogg' extension.");

        if (FileSystem.FileSystem.FindFileByName(
                FileSystem.FileSystem.MainDirPath,
                fileName)
            .HasErrored(out var pathError, out var path))
        {
            throw new ScriptRuntimeError(this, pathError);
        }

        if (!AudioClipStorage.LoadClip(path, name))
            throw new ScriptRuntimeError(this, "Audio has failed to load. Check the console for more info.");
    }
}
