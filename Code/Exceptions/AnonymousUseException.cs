namespace SER.Code.Exceptions;

public class AnonymousUseException(string reason) : SystemException(reason);