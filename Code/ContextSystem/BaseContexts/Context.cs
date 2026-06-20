using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem.BaseContexts;

public abstract class Context
{
    public required Script Script { get; set; } = null!;

    public abstract string FriendlyName { get; }

    public abstract TryAddTokenRes TryAddToken(BaseToken token);

    public abstract OldResult VerifyCurrentState();

    public abstract override string ToString();
}