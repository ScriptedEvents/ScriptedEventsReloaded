using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem.Contexts.Control;

[UsedImplicitly]
public class ReturnKeyword : YieldingContext, IKeywordContext
{
    private ValueExpressionContext? _expression = null;

    public override string FriendlyName => "'return' keyword";

    public string KeywordName => "return";
    public string Description => "Returns value when in a function.";
    public string[] Arguments => ["[return value]"];
    public string? Example => null;

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        if (_expression is not null) return _expression.TryAddToken(token);

        _expression = new ValueExpressionContext(token, true)
        {
            Script = token.Script
        };

        return TryAddTokenRes.Continue();
    }

    public override OldResult VerifyCurrentState()
    {
        if (_expression is not null) return _expression.VerifyCurrentState();
        return "Return value was not provided.";
    }

    protected override IEnumerator<float> Execute()
    {
        var coro = _expression!.Run();
        while (coro.MoveNext()) yield return coro.Current;
        
        if (_expression!.GetValue().HasErrored(out var error, out var value))
        {
            throw new ScriptRuntimeError(this, error);
        }

        ParentContext?.SendControlMessage(new Return(value));
    }
}