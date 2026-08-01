namespace SER.Code.Exceptions;

public class InternalSerException : SystemException
{
    protected InternalSerException(string component)
        : base($"Internal SER invariant failed in the {component} component")
    {
    }

    protected InternalSerException(string component, string error)
        : base($"Internal SER invariant failed in the {component} component: {error}")
    {
    }
}
