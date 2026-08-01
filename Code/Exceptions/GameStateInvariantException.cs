namespace SER.Code.Exceptions;

public class GameStateInvariantException : InternalSerException
{
    public GameStateInvariantException() : base("game-state")
    {
    }

    public GameStateInvariantException(string message) : base("game-state", message)
    {
    }
}
