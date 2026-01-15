using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;

namespace SER.Code.ContextSystem.Contexts.Control;

public class ReturnContext : StandardContext, IKeywordContext
{
    private IValueToken? _returnValueToken;
    
    public string KeywordName => "return";
    public string Description => "Returns value when in a function.";
    public string[] Arguments => ["[return value]"];

    protected override string FriendlyName => "'return' keyword";

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        if (token is not IValueToken valToken)
        {
            return TryAddTokenRes.Error(
                $"Expected to receive a value, but received '{token.RawRep}' instead."
            );
        }
        
        _returnValueToken = valToken;
        return TryAddTokenRes.End();
    }

    public override Result VerifyCurrentState()
    {
        return Result.Assert(
            _returnValueToken != null,
            "Return value was not provided."
        );
    }

    protected override void Execute()
    {
        if (_returnValueToken!.Value().HasErrored(out var error, out var value))
        {
            throw new ScriptRuntimeError(this, error);
        }
        
        ParentContext?.SendControlMessage(new Return(value));
    }
}