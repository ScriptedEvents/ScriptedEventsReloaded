using JetBrains.Annotations;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem.Contexts.Control;

[UsedImplicitly]
public class ElseStatement : StatementContext, IStatementExtender, IKeywordContext
{
    public string KeywordName => "else";
    public string Description =>
        "If the statement above it didn't execute, 'else' statement will execute instead.";
    public string[] Arguments => [];
    public string Example => throw new NotImplementedException();
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public IExtendableStatement.Signal Extends => IExtendableStatement.Signal.DidntExecute;

    protected override string FriendlyName => "'else' statement";

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
        foreach (var child in Children)
        {
            switch (child)
            {
                case YieldingContext yielding:
                {
                    var enumerator = yielding.Run();
                    while (enumerator.MoveNext())
                    {
                        yield return enumerator.Current;
                    }

                    break;
                }
                case StandardContext standard:
                    standard.Run();
                    break;
                default:
                    throw new AndrzejFuckedUpException();
            }
        }
    }
}