using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.AudioMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Speaker_ExistsMethod : ReturningMethod<BoolValue>
{
    public override string Description => "Returns true or false indicating if a speaker with the provided name exists.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("speaker name")
    ];

    public override void Execute()
    {
        ReturnValue = AudioPlayer.TryGet(Args.GetText("speaker name"), out _);
    }
}