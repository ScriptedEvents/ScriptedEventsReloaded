using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.CommunicationInterfaces;
using SER.Code.ContextSystem.Contexts.Control;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.Helpers;
using SER.Code.Helpers.Extensions;
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
    public static TryGet<Context[]> ContextLines(Line[] lines, Script scr)
    {
        Stack<StatementContext> statementStack = [];
        List<Context> contexts = [];
        
        foreach (var line in lines)
        {
            Result mainErr = $"Line {line.LineNumber} cannot compile.";
            if (ContextLine(line.Tokens, line.LineNumber, scr)
                .HasErrored(out var error, out var context))
            {
                return mainErr + error;
            }
            
            if (context is null) continue;
            
            if (TryAddResult(context, line.LineNumber, statementStack, contexts).HasErrored(out var addError))
            {
                return mainErr + addError;
            }
            Log.Debug($"current statement stack: {statementStack.Select(s => s.GetType().Name).JoinStrings(" -> ")}");
        }
        
        return contexts.ToArray();
    }

    private static Result TryAddResult(
        Context context,
        uint lineNum, 
        Stack<StatementContext> statementStack, 
        List<Context> contexts
    ) {
        Result rs = $"Invalid context {context} in line {lineNum}.";

        Log.Debug($"Trying to add context {context} in line {lineNum}");
        
        switch (context)
        {
            case EndStatementContext:
            {
                if (statementStack.Count == 0) 
                    return rs + "There is no statement to close with the 'end' keyword!";

                statementStack.Pop();
                return true;
            }
            case IRequireCurrentStatement rqsContext:
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

            extendable.RegisteredSignals[treeExtenderInfo.Extends] = treeExtenderContext.Run;
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

    public static TryGet<Context?> ContextLine(BaseToken[] tokens, uint? lineNum, Script scr)
    {
        Result rs = $"Line {(lineNum.HasValue ? $"{lineNum.Value} " : "")}cannot execute";
        
        var firstToken = tokens.FirstOrDefault();
        if (firstToken is null) return null as Context;
        
        if (firstToken is not IContextableToken contextable)
        {
            return rs + $"{firstToken} is not a valid way to start a line. Maybe you made a typo?";
        }

        var context = contextable.GetContext(scr);

        foreach (var token in tokens.Skip(1))
        {
            if (HandleCurrentContext(token, context, out var endLineContexting).HasErrored(out var errorMsg))
                return rs + errorMsg;

            if (endLineContexting) break;
        }

        return context;
    }

    private static Result HandleCurrentContext(BaseToken token, Context context, out bool endLineContexting)
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