namespace SER.Code.Exceptions;

public class DeveloperFuckedUpException : SystemException
{
    protected DeveloperFuckedUpException(string dev) : base($"Internal SER error in {dev}'s component")
    {
    }

    protected DeveloperFuckedUpException(string dev, string error)
        : base($"Internal SER error in {dev}'s component: {error}")
    {
    }
}
