using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem.BaseContexts;

public abstract class Context
{
    public required Script Script { get; set; } = null!;

    public virtual string Usage => this is SER.Code.ContextSystem.Interfaces.IKeywordContext keyword
        ? $"{keyword.KeywordName} {string.Join(" ", keyword.Arguments.Select(argument => argument.Syntax))}".TrimEnd()
        : FriendlyName;

    public abstract string FriendlyName { get; }

    public abstract TryAddTokenRes TryAddToken(BaseToken token);

    public abstract Result VerifyCurrentState();

    public abstract override string ToString();
}
