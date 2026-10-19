using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem.Contexts.Control;

[UsedImplicitly]
public class ElseStatement : StatementContext, IStatementExtender, IKeywordContext
{
    public override string FriendlyName => "'else' statement";
    public string KeywordName => "else";
    public string Description =>
        "If the statement above it didn't execute, 'else' statement will execute instead.";
    public ContextArgument[] Arguments => [];
    public string Example =>
        """
        if {@sender -> team} is "SCPs"
            Reply "You are an SCP"
        else
            Reply "You are not an SCP"
        end
        """;

    public IExtendableStatement.Signal Extends => IExtendableStatement.Signal.DidntExecute;

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        return TryAddTokenRes.Error("There should be no arguments after `else` keyword");
    }

    public override Result VerifyCurrentState()
    {
        return true;
    }

    protected override IEnumerator<float> Execute()
    {
        using var coro = RunChildren();
        while (coro.MoveNext())
        {
            yield return coro.Current;
        }
    }
}
