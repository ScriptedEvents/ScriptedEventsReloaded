using MEC;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.Helpers.FrameworkExtensions;

public sealed class CallvoteBridge : FrameworkBridge
{
    protected override string Name => "Callvote";
    public override IDependOnFramework.Type FrameworkType { get; } = IDependOnFramework.Type.Callvote;

    public CallvoteBridge()
    {
        
    }
}