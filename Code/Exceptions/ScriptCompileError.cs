namespace SER.Code.Exceptions;

public class ScriptCompileError(string error) : SystemException(error);