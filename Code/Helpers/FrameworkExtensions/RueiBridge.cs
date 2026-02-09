using MEC;

namespace SER.Code.Helpers.FrameworkExtensions;

public sealed class RueiBridge : FrameworkBridge
{
    public static event Action? OnDetected;
    protected override string Name { get; } = "RueI";
    
    public void Load()
    {
        Await(OnDetected).RunCoroutine();
    }
}