namespace SER.Code.Exceptions;

public class NotOurFaultException(string msg) : SystemException(msg)
{
    public NotOurFaultException() : this("An external game or plugin state did not match what SER expected.")
    {
    }
}
