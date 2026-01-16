using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.AudioMethods;

[UsedImplicitly]
public class IsAudioLoadedMethod : ReturningMethod<BoolValue>
{
    public override string Description => "Returns true if a given audio clip has been loaded";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("clip name")
        {
            Description = "This is NOT the path or the file name, but a clip name"
        }
    ];
    
    public override void Execute()
    {
        ReturnValue = AudioClipStorage.AudioClips.ContainsKey(Args.GetText("clip name"));
    }
}