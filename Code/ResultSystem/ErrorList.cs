using NorthwoodLib.Pools;

namespace SER.Code.ResultSystem;


public readonly struct ErrorList
{
    public ErrorList(List<string> errors)
    {
        Errors = errors;
    }

    public ErrorList(string error)
    {
        Errors = ListPool<string>.Shared.Rent(16);
        Errors.Add(error);
    }
    
    public List<string> Errors { get; }
}