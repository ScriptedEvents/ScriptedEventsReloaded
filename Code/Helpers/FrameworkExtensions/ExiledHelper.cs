using MEC;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.Helpers.FrameworkExtensions;

public sealed class ExiledBridge : FrameworkBridge
{
    protected override string Name => "Exiled Loader";
    public override bool ShouldRegister { get; } = false;
    public override IDependOnFramework.Type FrameworkType { get; } = IDependOnFramework.Type.Exiled;

    public ExiledBridge()
    {
        
    }
}