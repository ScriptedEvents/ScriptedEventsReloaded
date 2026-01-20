using JetBrains.Annotations;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.CommunicationInterfaces;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem;

namespace SER.Code.ContextSystem.Contexts.Control.Loops;

[UsedImplicitly]
public class RepeatLoopContext : LoopContextWithSingleIterationVariable<NumberValue>, IKeywordContext, IAcceptOptionalVariableDefinitions
{
    private readonly Result _rs = "Cannot create 'repeat' loop.";
    private Func<TryGet<ulong>>? _repeatCountExpression = null;
    private ulong? _repeatCount = null;

    public override Dictionary<IExtendableStatement.Signal, Func<IEnumerator<float>>> RegisteredSignals { get; } = [];
    
    public override string KeywordName => "repeat";
    public override string Description => "Repeats everything inside its body a given amount of times.";
    public override string[] Arguments => ["[number]"];

    protected override string FriendlyName => "'repeat' loop statement";

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        switch (token)
        {
            case NumberToken numberToken:
            {
                if (numberToken.Value < 0)
                {
                    return TryAddTokenRes.Error(
                        $"Value '{numberToken.Value}' cannot be negative.");
                }
                
                _repeatCount = (uint)numberToken.Value;
                return TryAddTokenRes.End();
            }
            
            case IValueToken valToken:
            {
                if (!valToken.CanReturn<NumberValue>(out var getNumber))
                {
                    return TryAddTokenRes.Error($"Value '{token.RawRep}' returns a value, but.");   
                }
                
                _repeatCountExpression = () =>
                {
                    if (getNumber().HasErrored(out var error, out var value))
                    {
                        return error;
                    }

                    if (value.Value < 0)
                    {
                        return $"Value '{value}' cannot be negative.";
                    }

                    return (uint)value.Value;
                };
                return TryAddTokenRes.End();
            }
        }

        return TryAddTokenRes.Error($"Value '{token.RawRep}' cannot be interpreted as a number.");
    }

    public override Result VerifyCurrentState()
    {
        return Result.Assert(
            _repeatCountExpression != null || _repeatCount.HasValue,
            _rs + "The amount of times to repeat was not provided."
        );
    }

    protected override IEnumerator<float> Execute()
    {
        if (!_repeatCount.HasValue)
        {
            if (_repeatCountExpression == null) 
                throw new AndrzejFuckedUpException("Repeat context has no amount specified");

            if (_repeatCountExpression().HasErrored(out var error, out var val))
            {
                throw new ScriptRuntimeError(this,  error);
            }
            
            _repeatCount = val;
        }

        for (ulong i = 0; i < _repeatCount.Value; i++)
        {
            SetVariable(i+1);
            var coro = RunChildren();
            while (coro.MoveNext())
            {
                yield return coro.Current;
            }

            RemoveVariable();
            if (ReceivedBreak) break;
        }
    }
}