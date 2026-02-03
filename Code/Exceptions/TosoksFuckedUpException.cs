namespace SER.Code.Exceptions;

public class TosoksFuckedUpException : DeveloperFuckedUpException
{
    public TosoksFuckedUpException() : base("tosoks")
    {
    }

    public TosoksFuckedUpException(string msg) : base("tosoks", msg)
    {
    }
}
