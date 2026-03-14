namespace SER.Code.Exceptions;

public class RetroReulFuckedUpException : DeveloperFuckedUpException
{
    public RetroReulFuckedUpException() : base("retroreul")
    {
    }
    
    public RetroReulFuckedUpException(string msg) : base("retroreul", msg)
    {
    }
}