namespace SER.Code.Exceptions;

public class ExecutionInvariantException : InternalSerException
{
    public ExecutionInvariantException() : base("execution")
    {
    }

    public ExecutionInvariantException(string message) : base("execution", message)
    {
    }
}
