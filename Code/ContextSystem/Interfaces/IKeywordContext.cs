namespace SER.Code.ContextSystem.Interfaces;

public interface IKeywordContext
{
    public string KeywordName { get; }
    public string Description { get; }
    public string[] Arguments { get; }
}