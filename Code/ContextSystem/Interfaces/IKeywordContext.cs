namespace SER.Code.ContextSystem.Interfaces;

public interface IKeywordContext
{
    public string KeywordName { get; }
    public string Description { get; }
    public SER.Code.ContextSystem.Structures.ContextArgument[] Arguments { get; }
    /// <summary>
    /// Canonical one-line invocation syntax used by help and editor tooling.
    /// </summary>
    public string Usage { get; }
    public string? Example { get; }
}
