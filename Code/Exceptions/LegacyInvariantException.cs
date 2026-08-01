namespace SER.Code.Exceptions;

public class LegacyInvariantException : InternalSerException
{
    public LegacyInvariantException() : base("legacy")
    {
    }

    public LegacyInvariantException(string message) : base("legacy", message)
    {
    }
}
