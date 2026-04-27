using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Contexts;
using SER.Code.ContextSystem.Contexts.Control;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.Helpers.ResultSystem;
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
            return rs + $"'{firstToken.RawRep}' is not a valid way to start a line. Perhaps you made a typo?";
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
}