using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.AudioMethods;

[UsedImplicitly]
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