using MEC;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.Helpers.FrameworkExtensions;

public sealed class UcrBridge : FrameworkBridge
{
    protected override string Name => "UncomplicatedCustomRoles";
    public override IDependOnFramework.Type FrameworkType { get; } = IDependOnFramework.Type.Ucr;

    public UcrBridge()
    {
        
    }
}