namespace SER.Code.Exceptions;

public class NotOurFaultException(string msg) : SystemException(msg)
{
    public NotOurFaultException() : this("Yeh that's not our fault trust")
    {
    }
}