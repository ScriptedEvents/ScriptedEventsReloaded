namespace SER.Code.MethodSystem.Structures;

/// <summary>
/// Marks that this method can only load when a given framework is loaded as well
/// </summary>
public interface IDependOnFramework
{
    public enum Type
    {
        None,
        Exiled,
        Callvote,
        Ucr,
        Ruei,
    }
    
    public Type DependsOn { get; }
}