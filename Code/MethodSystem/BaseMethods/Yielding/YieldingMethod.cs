namespace SER.Code.MethodSystem.BaseMethods.Yielding;

/// <summary>
///     Represents a SER method that can stop the execution of a script using its <see cref="IEnumerable{Float}" />.
/// </summary>
public abstract class YieldingMethod : Method
{
    public abstract IEnumerator<float> Execute();
}