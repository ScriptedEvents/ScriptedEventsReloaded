using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Contexts;
using SER.Code.ContextSystem.Contexts.Control;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.Helpers.ResultSystem;
using SER.Code.MethodSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Structures;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem;

/// <summary>
///     Responsible for joining tokens from a line together into contexts for execution.
/// </summary>
public static class Contexter
{
    private static List<string>? _suggestions = null;

    public static TryGet<RunnableContext[]> ContextLines(Line[] lines, Script scr)
    {
        Stack<StatementContext> statementStack = [];
        List<RunnableContext> contexts = [];

        List<Result> errors = [];
        foreach (var line in lines)
        {
            Result mainErr = $"Line {line.LineNumber} cannot compile.";
            if (ContextLine(line.Tokens, line.LineNumber, scr)
                .HasErrored(out var error, out var context))
            {
                errors.Add(mainErr + error);
                continue;
            }

            if (context is null) continue;

            if (TryAddResult(context, line.LineNumber, statementStack, contexts).HasErrored(out var addError))
            {
                errors.Add(mainErr + addError);
                continue;
            }
            Log.Debug($"current statement stack: {statementStack.Select(s => s.GetType().Name).JoinStrings(" -> ")}");
        }

        if (errors.Any()) return Result.Merge(errors);

        return contexts.ToArray();
    }

    private static Result TryAddResult(
        RunnableContext context,
        uint lineNum,
        Stack<StatementContext> statementStack,
        List<RunnableContext> contexts
    )
    {
        Result rs = $"Invalid {context}";

        Log.Debug($"Trying to add context {context}");

        switch (context)
        {
            case EndKeyword:
            {
                if (statementStack.Count == 0)
                    return rs +
                           "Check if the statement you are trying to close hasn't thrown an error when compiling.".AsError() +
                           "There is no valid statement to close with the 'end' keyword!".AsError();

                var lastContext = statementStack.Pop();
                lastContext.EndLine = context.LineNum;
                return true;
            }
            case IRequirePreviousStatementContext rqsContext:
            {
                if (statementStack.Count == 0)
                {
                    return rs + $"{context} expected to be inside a statement, but it isn't.";
                }

                if (rqsContext.AcceptStatement(statementStack.Peek()).HasErrored(out var asError))
                {
                    return rs + asError;
                }

                break;
            }
        }

        var currentStatement = statementStack.FirstOrDefault();
        string error;
        if (context is StatementContext treeExtenderContext and IStatementExtender treeExtenderInfo)
        {
            if (currentStatement is null)
            {
                return rs + "There is no statement to extend.";
            }

            if (currentStatement is not IExtendableStatement extendable)
            {
                return rs + "The statement to extend is not extendable.";
            }

            if (!extendable.AllowedSignals.HasFlag(treeExtenderInfo.Extends))
            {
                return rs + "The statement to extend does not support this type of extension.";
            }

            extendable.RegisteredSignals[treeExtenderInfo.Extends] = treeExtenderContext;
            statementStack.Pop();
            statementStack.Push(treeExtenderContext);
            return context.VerifyCurrentState().HasErrored(out error) ? rs + error.AsError() : true;
        }

        if (currentStatement is not null)
        {
            Log.Debug($"Adding finished context {context} to tree context {currentStatement}");
            currentStatement.Children.Add(context);
            context.ParentContext = currentStatement;
        }
        else
        {
            Log.Debug($"Adding finished context {context} to main collection");
            contexts.Add(context);
        }

        if (context is StatementContext treeContext)
            statementStack.Push(treeContext);

        if (context.VerifyCurrentState().HasErrored(out error))
            return rs + error;

        Log.Debug($"Line {lineNum} has been contexted to {context}");
        return true;
    }

    public static TryGet<RunnableContext?> ContextLine(BaseToken[] tokens, uint? lineNum, Script scr)
    {
        Result rs = $"Line {(lineNum.HasValue ? $"{lineNum.Value} " : "")}is invalid";

        var firstToken = tokens.FirstOrDefault();
        if (firstToken is null) return null as RunnableContext;

        if (firstToken is not IContextableToken contextable)
        {
            var tip = $"'{firstToken.RawRep}' is not a valid way to start a line."
                      + (FindClosestMatches(firstToken.RawRep) is { Length: > 0 } matches
                          ? $" Did you mean to use {matches.Select(x => $"'{x}'").JoinStrings(" or ")}?"
                          : "");
            return rs + tip.AsError();
        }

        var context = contextable.GetContext(scr);
        if (context is null) return context;

        var endLineContexting = false;
        for (var index = 1; index < tokens.Length; index++)
        {
            var token = tokens[index];
            rs = $"{token} was rejected by {context}";
            if (AttemptsInlineWithKeyword(token, context))
            {
                if (HandleInlineWithKeyword(tokens.Skip(index), context, scr).HasErrored(out var error))
                {
                    return rs + error;
                }

                break;
            }

            if (endLineContexting)
            {
                break;
            }

            if (token is CommentToken)
            {
                return context;
            }

            if (HandleCurrentContext(token, context, out endLineContexting).HasErrored(out var errorMsg))
                return rs + errorMsg;
        }

        return context;
    }

    private static bool AttemptsInlineWithKeyword(BaseToken token, Context currentContext)
    {
        return token is IContextableToken contextable
               && contextable.GetContext(token.Script) is WithKeyword
               && currentContext is StatementContext;
    }

    private static Result HandleInlineWithKeyword(IEnumerable<BaseToken> enumTokens, RunnableContext context, Script scr)
    {
        var tokens = enumTokens.ToArray();

        if (tokens.First() is not IContextableToken contextable2
            || contextable2.GetContext(scr) is not WithKeyword
            || context is not StatementContext statement)
        {
            return $"{context.FriendlyName} does not accept {tokens.First()}";
        }

        if (ContextLine(tokens, null, scr).HasErrored(out var contextError, out var contextResult))
        {
            return contextError;
        }

        if (contextResult is not WithKeyword with)
        {
            return $"{contextResult.FriendlyName} does not accept {tokens.First()}";
        }

        if (with.AcceptStatement(statement).HasErrored(out var acceptError))
        {
            return acceptError;
        }

        if (with.VerifyCurrentState().HasErrored(out var verifyError))
        {
            return verifyError;
        }

        return true;
    }

    private static Result HandleCurrentContext(BaseToken token, RunnableContext context, out bool endLineContexting)
    {
        Log.Debug($"Handling token {token} in context {context}");

        var result = context.TryAddToken(token);
        if (result.HasErrored)
        {
            endLineContexting = true;
            return result.ErrorMessage;
        }

        if (result.ShouldContinueExecution)
        {
            endLineContexting = false;
            return true;
        }

        endLineContexting = true;
        return true;
    }
    
    public static string[] FindClosestMatches(string input)
    {
        if (_suggestions is null)
        {
            _suggestions = [];
            _suggestions.AddRange(MethodIndex.GetMethodNames());
            _suggestions.AddRange(ContextableKeywordToken.KeywordContexts.Select(k => k.KeywordName));
        }

        return _suggestions
            .Select(name => new { Name = name, Score = GetDiceCoefficient(input, name) })
            .Where(x => x.Score > 0.5)
            .OrderByDescending(x => x.Score)
            .Take(3)
            .Select(x => x.Name)
            .ToArray();
    }

    public static double GetDiceCoefficient(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return 0;
        if (s1 == s2) return 1;

        var s1Upper = s1.ToUpperInvariant();
        var s2Upper = s2.ToUpperInvariant();

        var set1 = Enumerable.Range(0, s1Upper.Length - 1).Select(i => s1Upper.Substring(i, 2)).ToList();
        var set2 = Enumerable.Range(0, s2Upper.Length - 1).Select(i => s2Upper.Substring(i, 2)).ToList();

        var matches = 0;
        foreach (var bigram in set1)
        {
            if (set2.Remove(bigram)) matches++;
        }

        return 2.0 * matches / (s1.Length - 1 + s2.Length - 1);
    }
}