using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.ContextSystem.Contexts;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.ScriptSystem.Structures;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.ArgumentSystem.Arguments;

public class CallbackArgument(string argumentName, params (SingleTypeOfValue type, string name)[] requiredArguments) 
    : Argument(argumentName)
{
    public class Callback
    {
        public required Action<Value[], Action<Script>?> Action;
        public required string Name;
    }
    
    public string FuncName = null!;
    
    public override string InputDescription => 
        "A name of a function defined above e.g. MyFunction" +
        (requiredArguments.Length > 0 
            ? $". It has to have these arguments: {requiredArguments
                .Select(x => $"{Value.GetPrefixOfValue(x.type)}{x.name}").JoinStrings(" ")}"
            : string.Empty
        );
    
    [UsedImplicitly]
    public DynamicTryGet<Callback> GetConvertSolution(BaseToken token)
    {
        if (token.BestTextRepr().IsStatic(out var value, out var func))
        {
            return Verify(value);
        }
        
        return new(() => Verify(func()));
    }

    private TryGet<Callback> Verify(string funcName)
    {
        if (FetchFunc(Script, funcName).HasErrored(out var err))
        {
            return err;
        }
        
        FuncName = funcName;
        return new(
            new Callback
            {
                Action = GetCallback, 
                Name = funcName
            }, 
            null
        );
    }

    private void GetCallback(Value[] args, Action<Script>? modifier)
    {
        var mainError = $"Failed getting the callback function '{FuncName}' from script '{Script.Name}'.".AsError();
        
        // create new instance of the script since there may have been changes to it, 
        // which the old object will not know about
        if (Script.Name.GetScript(null).HasErrored(out var error, out var hostScript) 
            || hostScript.Compile().HasErrored(out error) 
            || FetchFunc(hostScript, FuncName).HasErrored(out error, out var func))
        {
            throw new CustomScriptRuntimeError(mainError + error.AsError());
        }

        if (func.LineNum is not { } startLine)
        {
            throw new CustomScriptRuntimeError(
                mainError 
                + $"Cannot find the beginning of the '{FuncName}' function - this should not happen.".AsError());
        }

        if (func.EndLine is not { } endLine)
        {
            throw new CustomScriptRuntimeError(
                mainError
                + $"Cannot find the end of the '{FuncName}' function - this should not happen.".AsError());
        }
        
        var funcContent = hostScript
            .Content
            .Replace("\r", string.Empty)
            .Split('\n')
            .AsSpan((int)startLine, (int)endLine - (int)startLine - 1)
            .ToArray()
            .JoinStrings("\n");

        var executingScript = Script.CreateForCallback(
            $"'{func.FunctionName}' function from '{Script.Name}' script",
            funcContent,
            Script.Executor,
            scr =>
            {
                for (int i = 0; i < args.Length; i++)
                {
                    scr.AddLocalVariable(
                        Variable.Create(func.ExpectedVariables[i].Name, args[i])
                    );
                }
            }
        );

        if (modifier is null)
        {
            executingScript.Run(RunReason.FunctionCallback, Script);
        }
        else
        {
            modifier.Invoke(executingScript);
        }
    }

    private TryGet<FuncStatement> FetchFunc(Script functionHolder, string functionName)
    {
        if (!functionHolder.DefinedFunctions.TryGetValue(functionName, out var func))
        {
            return $"There is no function called '{functionName}'";
        }

        if (func.ExpectedVariables.Length != requiredArguments.Length)
        {
            return $"The amount of expected variables in the '{functionName}' function is {func.ExpectedVariables.Length}, " +
                   $"but {requiredArguments.Length} are needed.";
        }

        for (int i = 0; i < requiredArguments.Length; i++)
        {
            var requiredType = requiredArguments[i].type;
            var definedType = func.ExpectedVariables[i].ValueType;

            if (!definedType.CanHold(requiredType))
            {
                return $"Method expects the argument #{i + 1} to be of '{requiredType}', " +
                       $"but '{functionName}' function defines it to be of '{definedType}'.";
            }
        }

        return func;
    }
}