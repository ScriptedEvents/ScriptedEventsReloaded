using JetBrains.Annotations;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Structures;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem;

namespace SER.Code.ContextSystem.Contexts.Control;

[UsedImplicitly]
public class ReturnKeyword : StandardContext, IKeywordContext
{
    private IValueToken? _returnValueToken;
    private (Context main, IMayReturnValueContext returner)? _returnContext = null; 
    
    public string KeywordName => "return";
    public string Description => "Returns value when in a function.";
    public string[] Arguments => ["[return value]"];
    public string? Example => null;

    protected override string FriendlyName => "'return' keyword";
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        if (_returnContext.HasValue)
        {
            return _returnContext.Value.main.TryAddToken(token);
        }
        
        switch (token)
        {
            case IContextableToken contextable when
                contextable.GetContext(Script) is { } mainContext and IMayReturnValueContext returnValueContext:
            {
                _returnContext = (mainContext, returnValueContext);
                return TryAddTokenRes.Continue();
            }
            case IValueToken valToken:
            {
                _returnValueToken = valToken;
                return TryAddTokenRes.End();
            }
            default:
                return TryAddTokenRes.Error($"Expected to receive a value or method, but received '{token.RawRep}' instead.");
        }
    }

    public override Result VerifyCurrentState()
    {
        return Result.Assert(
            _returnValueToken != null || _returnContext.HasValue,
            "Return value was not provided."
        );
    }

    protected override void Execute()
    {
        Value value;
        if (_returnContext.HasValue)
        {
            value = _returnContext.Value.returner.ReturnedValue
                    ?? throw new ScriptRuntimeError(this,
                        $"{_returnContext.Value.main} has not returned a value. " +
                        $"{_returnContext.Value.returner.MissingValueHint}"
                    );
        }
        else if (_returnValueToken!.Value().HasErrored(out var error, out value!))
        {
            throw new ScriptRuntimeError(this, error);
        }
        
        ParentContext?.SendControlMessage(new Return(value));
    }
}