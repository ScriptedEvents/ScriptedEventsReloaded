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
/// Responsible for joining tokens from a line together into contexts for execution.
/// </summary>
public static class Contexter
{
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
    ) {
        Result rs = $"Invalid {context}";

        Log.Debug($"Trying to add context {context}");
        
        switch (context)
        {
            case EndKeyword:
            {
                if (statementStack.Count == 0) return 
                    rs +
                    "There is no valid statement to close with the 'end' keyword! " +
                    "Check if the statement you are trying to close hasn't thrown an error when compiling.".AsError();

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
            return context.VerifyCurrentState().HasErrored(out error) ? rs + error : true;
        }

        if (context.VerifyCurrentState().HasErrored(out error)) 
            return rs + error;

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
            var text = $"'{firstToken.RawRep}' is not a valid way to start a line."
                + (FindClosestMatch(firstToken.RawRep) is { } match ? $" Did you mean to use '{match}'?" : "");
            return rs + text.AsError();
        }

        var context = contextable.GetContext(scr);
        if (context is null) return context;
        
        bool endLineContexting = false;
        for (var index = 1; index < tokens.Length; index++)
        {
            var token = tokens[index];
            rs = $"Cannot add token {token} to {context}";
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
        Result rs = $"Cannot add '{token.RawRep}' to {context}";
        Log.Debug($"Handling token {token} in context {context}");

        var result = context.TryAddToken(token);
        if (result.HasErrored)
        {
            endLineContexting = true;
            return rs + result.ErrorMessage;
        }

        if (result.ShouldContinueExecution)
        {
            endLineContexting = false;
            return true;
        }

        endLineContexting = true;
        return true;
    }

    private static List<string>? _suggestions = null;
    public static string? FindClosestMatch(string input)
    {
        if (_suggestions is null)
        {
            _suggestions = [];
            _suggestions.AddRange(MethodIndex.GetMethods().Select(m => m.Name));
            _suggestions.AddRange(KeywordToken.KeywordContexts.Select(k => k.KeywordName));
        }
        
        var suggestion = _suggestions
            .Select(name => new { Name = name, Score = GetJaroWinklerSimilarity(input, name) })
            .Where(x => x.Score > 0.5)
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();

        return suggestion?.Name;
    }
    
    public static double GetJaroWinklerSimilarity(string s1, string s2)
    {
        s1 = s1.ToLowerInvariant();
        s2 = s2.ToLowerInvariant();
        
        double jaroDist = GetJaroSimilarity(s1, s2);
        if (jaroDist < 0.7) return jaroDist;
        
        int prefixLength = 0;
        for (int i = 0; i < Math.Min(4, Math.Min(s1.Length, s2.Length)); i++)
        {
            if (s1[i] == s2[i]) prefixLength++;
            else break;
        }

        return jaroDist + prefixLength * 0.1 * (1.0 - jaroDist);
    }

    private static double GetJaroSimilarity(string s1, string s2)
    {
        int s1Len = s1.Length, s2Len = s2.Length;
        if (s1Len == 0 && s2Len == 0) return 1.0;
    
        int matchDistance = Math.Max(s1Len, s2Len) / 2 - 1;
        bool[] s1Matches = new bool[s1Len];
        bool[] s2Matches = new bool[s2Len];

        int matches = 0;
        for (int i = 0; i < s1Len; i++)
        {
            int start = Math.Max(0, i - matchDistance);
            int end = Math.Min(i + matchDistance + 1, s2Len);
            for (int j = start; j < end; j++)
            {
                if (s2Matches[j] || s1[i] != s2[j]) continue;
                s1Matches[i] = true;
                s2Matches[j] = true;
                matches++;
                break;
            }
        }

        if (matches == 0) return 0.0;

        double transpositions = 0;
        int k = 0;
        for (int i = 0; i < s1Len; i++)
        {
            if (!s1Matches[i]) continue;
            while (!s2Matches[k]) k++;
            if (s1[i] != s2[k]) transpositions++;
            k++;
        }

        return (matches / (double)s1Len + matches / (double)s2Len + (matches - transpositions / 2.0) / matches) / 3.0;
    }
}