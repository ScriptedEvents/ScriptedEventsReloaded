using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem.BaseContexts;

public abstract class Context
{
    public required Script? Script { get; set; } = null!;

    public required uint? LineNum { get; set; }

    public StatementContext? ParentContext { get; set; } = null;

    protected abstract string FriendlyName { get; }

    private readonly List<BaseToken> _tokens = [];
    public BaseToken[] Tokens => _tokens.ToArray();

    public TryAddTokenRes TryAddToken(BaseToken token)
    {
        var result = OnAddingToken(token);
        if (!result.HasErrored)
        {
            _tokens.Add(token);
        }
        
        return result;
    }
    
    protected abstract TryAddTokenRes OnAddingToken(BaseToken token);

    public abstract Result VerifyCurrentState();

    public static Context Create(Type contextType, Script? scr, uint? lineNum)
    {
        var context = (Context)Activator.CreateInstance(contextType);
        context.Script = scr;
        context.LineNum = lineNum;
        return context;
    }

    public override string ToString()
    {
        if (LineNum.HasValue) 
            return $"{FriendlyName} at line {LineNum}";
        
        return FriendlyName;
    }
}