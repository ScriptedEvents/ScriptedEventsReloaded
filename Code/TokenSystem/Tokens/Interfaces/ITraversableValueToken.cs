using SER.Code.Helpers.ResultSystem;
using SER.Code.ValueSystem;

namespace SER.Code.TokenSystem.Tokens.Interfaces;

public interface ITraversableValueToken
{
    public TryGet<Value[]> GetTraversableValues();
}