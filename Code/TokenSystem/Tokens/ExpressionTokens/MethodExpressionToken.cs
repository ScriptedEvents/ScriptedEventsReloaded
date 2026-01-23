using SER.Code.ContextSystem;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.TokenSystem.Tokens.ExpressionTokens;

public class MethodExpressionToken : ExpressionToken
{
    private ReturningMethod? _method = null!;

    //public override TypeOfValue PossibleValues => new UnknownTypeOfValue();
    public override TypeOfValue PossibleValues => _method!.Returns;

    protected override IParseResult InternalParse(BaseToken[] tokens)
    {
        if (tokens.FirstOrDefault() is not MethodToken methodToken)
        {
            return new Ignore();
        }

        if (methodToken.Method is not ReturningMethod method)
        {
            return new Error($"Method '{methodToken.Method.Name}' does not return a value.");
        }
        
        if (Contexter.ContextLine(tokens, null, Script).HasErrored(out var contextError))
        {
            return new Error(contextError);
        }
        
        _method = method;
        return new Success();
    }

    public override TryGet<Value> Value()
    {
        if (_method is null) throw new AndrzejFuckedUpException();
        
        _method.Execute();
        return _method.ReturnValue ?? throw new AndrzejFuckedUpException();
    }
}