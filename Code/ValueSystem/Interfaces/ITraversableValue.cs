using SER.Code.Helpers.ResultSystem;

namespace SER.Code.ValueSystem.Interfaces;

public interface ITraversableValue
{
    public TryGet<Value[]> TryGetValues();
}