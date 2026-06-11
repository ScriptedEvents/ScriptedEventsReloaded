namespace SER.Code.MethodSystem.Structures;

/// <summary>
///     Used to define an alias for tokenizer to recognize that alias as this method.
///     Used when replacing an old method with a new one.
///     These aliases work only for the method token implementation, documentation does not support them,
///     because they are supposed to be used when removing functionality while not breaking servers.
/// </summary>
public interface IHasAliases
{
    public string[] Aliases { get; }
}