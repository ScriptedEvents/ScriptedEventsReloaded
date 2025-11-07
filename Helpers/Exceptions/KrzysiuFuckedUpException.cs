using System;

namespace SER.Helpers.Exceptions;
public class KrzysiuFuckedUpException : SystemException
{
    public KrzysiuFuckedUpException()
    {
    }

    public KrzysiuFuckedUpException(string msg) : base(msg)
    {
    }
}
