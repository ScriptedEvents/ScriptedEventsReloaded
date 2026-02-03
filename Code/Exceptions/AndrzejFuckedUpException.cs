namespace SER.Code.Exceptions;

public class AndrzejFuckedUpException : DeveloperFuckedUpException
{
    public AndrzejFuckedUpException() : base("andrzej")
    {
    }
    
    public AndrzejFuckedUpException(string msg) : base("andrzej", msg)
    {
    }
}