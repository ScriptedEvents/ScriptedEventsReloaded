namespace SER.Code.ContextSystem.Interfaces;

/// <summary>
/// Automatically creates a keyword and connects it to the context.
/// </summary>
public interface IKeywordContext
{
    public string KeywordName { get; }
    public string Description { get; }
    public string[] Arguments { get; }
    
    //public string ExampleUsage { get; }
}