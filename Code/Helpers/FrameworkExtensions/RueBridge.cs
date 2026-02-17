using MEC;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.Helpers.FrameworkExtensions;

public sealed class RueBridge : FrameworkBridge
{
    protected override string Name { get; } = "RueI";
    public override IDependOnFramework.Type FrameworkType { get; } = IDependOnFramework.Type.Ruei;

    public RueBridge()
    {
        
    }
}