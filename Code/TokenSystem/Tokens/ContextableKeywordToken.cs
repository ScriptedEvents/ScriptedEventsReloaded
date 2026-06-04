using System.Reflection;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.Extensions;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Structures;

namespace SER.Code.TokenSystem.Tokens;

public class ContextableKeywordToken : BaseToken, IContextableToken
{
    private Type? _keywordType = null;
    
    public static readonly Type[] KeywordContextTypes = Assembly.GetExecutingAssembly().GetTypes()
        .Where(t => 
            t.IsClass && 
            !t.IsAbstract && 
            typeof(IKeywordContext).IsAssignableFrom(t) &&
            typeof(RunnableContext).IsAssignableFrom(t)
        )
        .ToArray();
    
    public static readonly IKeywordContext[] KeywordContexts = KeywordContextTypes.Select(t => t.CreateInstance<IKeywordContext>()).ToArray();
    
    protected override IParseResult InternalParse(Script scr)
    {
        if (RawRep is "foreach")
        {
            return new Error(
                "The 'foreach' keyword has been replaced with 'over' keyword in the newest update. " +
                "Please update your script."
            );
        }
        
        _keywordType = KeywordContexts.FirstOrDefault(keyword => keyword.KeywordName == RawRep)?.GetType();
        return _keywordType is not null
            ? new Success()
            : new Ignore();
    }

    public RunnableContext GetContext(Script scr)
    {
        return RunnableContext.Create(_keywordType!, scr, LineNum);
    }
}