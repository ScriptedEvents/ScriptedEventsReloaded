using System.Reflection;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.Helpers.Extensions;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Structures;

namespace SER.Code.TokenSystem.Tokens;

public class KeywordToken : BaseToken, IContextableToken
{
    private Type? _keywordType = null;
    
    public static readonly Type[] KeywordContextTypes = Assembly.GetExecutingAssembly().GetTypes()
        .Where(t => 
            t.IsClass && 
            !t.IsAbstract && 
            typeof(IKeywordContext).IsAssignableFrom(t) &&
            typeof(Context).IsAssignableFrom(t)
        )
        .ToArray();
    
    protected override IParseResult InternalParse(Script scr)
    {
        _keywordType = KeywordContextTypes.FirstOrDefault(
            keyword => keyword.CreateInstance<IKeywordContext>().KeywordName == RawRep);

        return _keywordType is not null
            ? new Success()
            : new Ignore();
    }

    public Context GetContext(Script scr)
    {
        return Context.Create(_keywordType!, (scr, LineNum));
    }
}