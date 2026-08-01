namespace SER.Code.Exceptions;

public class CoreInvariantException : InternalSerException
{
    public CoreInvariantException() : base("core")
    {
    }

    public CoreInvariantException(string message) : base("core", message)
    {
    }
}
