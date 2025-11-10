using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;

namespace SER.MethodSystem.Methods.AudioMethods;

public class DestroySpeakerMethod : SynchronousMethod
{
    public override string Description => "Destorys a speaker.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("speaker name"),
    ];
    
    public override void Execute()
    {
        if (AudioPlayer.TryGet(Args.GetText("speaker name"), out var ap))
        {
            UnityEngine.Object.Destroy(ap);
        }
    }
}