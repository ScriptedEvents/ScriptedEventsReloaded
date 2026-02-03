namespace SER.Code.Exceptions;

public class DeveloperFuckedUpException : SystemException
{
    protected DeveloperFuckedUpException(string dev) : base($"{dev} fucked up")
    {
    }

    protected DeveloperFuckedUpException(string dev, string error) : base($"{dev} fucked up: {error}")
    {
    }
}