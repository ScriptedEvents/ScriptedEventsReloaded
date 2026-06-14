using System.Collections.ObjectModel;
using LabApi.Features.Wrappers;
using SER.Code.ContextSystem;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Contexts;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.FlagSystem;
using SER.Code.FlagSystem.Flags;
using SER.Code.Helpers;
using SER.Code.Helpers.ResultSystem;
using SER.Code.MethodSystem;
using SER.Code.ScriptSystem.Structures;
using SER.Code.TokenSystem;
using SER.Code.TokenSystem.Structures;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.VariableSystem;
using SER.Code.VariableSystem.Bases;
using SER.Code.VariableSystem.Structures;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.ScriptSystem;

public class Script
{
    private Line[] _lines = [];
    private RunnableContext[] _contexts = [];
    public bool IsEventAllowed = true;
    
    public required ScriptName Name { get; init; }
    
    public required string Content { get; init; }
    
    public required ScriptExecutor Executor
    {
        get;
        init
        {
            switch (value)
            {
                case RemoteAdminExecutor { Sender: { } sender } when Player.Get(sender) is { } player:
                {
                    AddLocalVariable(new PlayerVariable("sender", new(player)));
                    break;
                }
                case PlayerConsoleExecutor { Sender: { } hub } when Player.Get(hub) is { } player:
                {
                    AddLocalVariable(new PlayerVariable("sender", new(player)));
                    break;
                }
            }

            field = value;
        }
    }
    
    public bool Killed { get; private set; }
    
    public Script? Caller { get; private set; }

    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    public Profile? Profile { get; private set; }
    
    public RunReason RunReason { get; private set; }
    
    public uint CurrentLine { get; set; }
    
    public DateTime StartTime { get; private set; }

    public TimeSpan TimeRunning => StartTime == default ? TimeSpan.Zero : DateTime.Now - StartTime;

    private static readonly HashSet<Script> RunningScriptsList = [];
    public static Script[] RunningScripts => RunningScriptsList.ToArray();
    
    private readonly Dictionary<(char, string), Variable> _localVariables = [];
    public Variable[] LocalVariables => _localVariables.Values.ToArray();

    private readonly Dictionary<string, FuncStatement> _definedFunctions = [];
    public ReadOnlyDictionary<string, FuncStatement> DefinedFunctions => new(_definedFunctions);

    private Action<Script>? _beforeExecutionAction;
    
    public void Reply(string message)
    {
        Executor.Reply(message, this);
    }
    
    public void Warn(string message)
    {
        Executor.Warn(message, this);
    }
    
    public void Error(string message)
    {
        Executor.Error(message, this);
    }

    public static TryGet<Script> CreateByScriptName(ScriptName name, ScriptExecutor? executor)
    {
        if (FileSystem.FileSystem.GetScriptPath(name).HasErrored(out var error, out var path))
        {
            return error;
        }
        
        return new Script
        {
            Name = name,
            Content = File.ReadAllText(path),
            Executor = executor ?? ScriptExecutor.Get()
        };
    }
    
    public static TryGet<Script> CreateByScriptName(string dirtyName, ScriptExecutor? executor)
    {
        var name = Path.GetFileNameWithoutExtension(dirtyName);
        if (ScriptName.Create(name).HasErrored(out var initError, out var scriptName))
        {
            return initError;       
        }
        
        if (FileSystem.FileSystem.GetScriptPath(scriptName).HasErrored(out var error, out var path))
        {
            return error;
        }

        return new Script
        {
            Name = scriptName,
            Content = File.ReadAllText(path),
            Executor = executor ?? ScriptExecutor.Get()
        };
    }
    
    public static Script CreateByVerifiedPath(string path, ScriptExecutor? executor) => new() 
    {
        Name = ScriptName.CreateUnsafe(Path.GetFileNameWithoutExtension(path)),
        Content = File.ReadAllText(path),
        Executor = executor ?? ScriptExecutor.Get()
    };

    public static Script CreateAnonymous(string name, string content) => new()
    {
        Name = ScriptName.CreateUnsafe(name),
        Content = content,
        Executor = ScriptExecutor.Get()
    };
    
    public static Script CreateForCallback(
        string name, 
        string content, 
        ScriptExecutor executor, 
        Action<Script>? beforeExecutionAction = null)
    {
        return new Script
        {
            Name = ScriptName.CreateUnsafe(name),
            Executor = executor,
            Content = content,
            _beforeExecutionAction = beforeExecutionAction
        };
    }

    public static int StopAll()
    {
        var count = RunningScripts.Length;
        foreach (var script in new List<Script>(RunningScripts))
        {
            script.MarkAsStopped();
        }

        return count;
    }

    public void MarkAsStopped()
    {
        Killed = true;
        RunningScriptsList.Remove(this);
    }
    
    public static int StopByName(string name)
    {
        var matches = new List<Script>(RunningScripts)
            .Where(scr => string.Equals(scr.Name, name, StringComparison.CurrentCultureIgnoreCase))
            .ToArray();
        
        matches.ForEachItem(scr => scr.MarkAsStopped());
        return matches.Length;
    }

    public bool HasFlag<T>() where T : Flag
    {
        return ScriptFlagHandler.ScriptsFlags[Name].Any(f => f is T);
    }

    public TryGet<List<Line>> GetFlagLines()
    {
        DefineLines();
        if (SliceLines().HasErrored(out var err) || TokenizeLines().HasErrored(out err))
        {
            return err;
        }

        return _lines.Where(l => l.Tokens.FirstOrDefault() is FlagToken or FlagArgumentToken).ToList();
    }

    /// <summary>
    /// Used for external tools to verify the script content.
    /// This is NOT to be used in an actual server.
    /// </summary>
    [UsedImplicitly]
    public static string? VerifyForExternalTool(string content, string? name = null)
    {
        if (MethodIndex.NameToMethodIndex.Count is 0)
        {
            MethodIndex.Initialize();
        }
        
        return CreateAnonymous(name ?? "test", content).Compile().HasErrored(out var err) ? err : null;
    }

    /// <summary>
    /// Executes the script.
    /// </summary>
    public void Run(RunReason reason = RunReason.Unknown, Script? caller = null)
    {
        RunForEvent(reason, caller);
    }

    /// <summary>
    /// Executes the script.
    /// </summary>
    /// <returns>Returns a boolean indicating whether the event is allowed.</returns>
    public bool? RunForEvent(RunReason reason, Script? caller = null)
    {
        StartTime = DateTime.Now;
        RunReason = reason;
        Caller = caller;
        
        if (ScriptFlagHandler.DoFlagsApproveExecution(this, out var mustReport).HasErrored(out var error))
        {
            if (mustReport) Executor.Error(error, this);
            return null;
        }
        
        _beforeExecutionAction?.Invoke(this);
        
        RunningScriptsList.Add(this);
        //Profile = new Profile(this);
        InternalExecute().Run(
            this,
            null,
            MarkAsStopped
        );
        
        return IsEventAllowed;
    }

    public void DefineLines()
    {
        var prof = Profile is not null 
            ? new Profile(Profile, nameof(DefineLines))
            : null;
        
        _lines = Tokenizer.GetInfoFromMultipleLines(Content);
        
        Log.Debug($"Script {Name} defines {_lines.Length} lines");
        prof?.Stop();
    }
    
    public Result SliceLines()
    {
        var prof = Profile is not null 
            ? new Profile(Profile, nameof(SliceLines))
            : null;
        
        List<Result> errors = [];
        foreach (var line in _lines)
        {
            if (Tokenizer.SliceLine(line).HasErrored(out var error))
            {
                errors.Add(error);
            }
        }

        if (errors.Any())
        {
            return Result.Merge(errors);
        }
        
        prof?.Stop();
        
        Log.Debug($"Script {Name} sliced {_lines.Length} lines into {_lines.Sum(l => l.Slices.Length)} slices");
        
        return true;
    }

    public Result TokenizeLines()
    {
        var prof = Profile is not null 
            ? new Profile(Profile, nameof(TokenizeLines))
            : null;
        
        List<Result> errors = [];
        foreach (var line in _lines)
        {
            if (Tokenizer.TokenizeLine(line, this).HasErrored(out var error))
            {
                errors.Add(error);
            }
        }
        
        prof?.Stop();
        if (errors.Any()) return Result.Merge(errors);

        Log.Debug($"Script {Name} tokenized {_lines.Length} lines into {_lines.Sum(l => l.Tokens.Length)} tokens");
        return true;
    }
    
    public void DefineFunction(string name, FuncStatement context) 
        => _definedFunctions.Add(name, context);
    
    private Result ContextLines()
    {
        var prof = Profile is not null 
            ? new Profile(Profile, nameof(ContextLines))
            : null;
        
        if (Contexter.ContextLines(_lines, this).HasErrored(out var err, out var contexts))
        {
            return err;
        }
        
        prof?.Stop();
        
        _contexts = contexts;
        return true;
    }
    
    public Result Compile()
    {
        DefineLines();
        if (SliceLines().HasErrored(out var err) ||
            TokenizeLines().HasErrored(out err) ||
            ContextLines().HasErrored(out err))
        {
            return err;
        }

        return true;
    }
    
    public void CompileWithAutomaticThrow()
    {
        if (Compile().HasErrored(out var err))
        {
            throw new ScriptCompileError(err);
        }
    }

    private IEnumerator<float> InternalExecute()
    {
        if (_contexts.Length is 0)
        {
            if (Compile().HasErrored(out var err))
            {
                throw new ScriptCompileError(err);
            }
        }
        
        foreach (var context in _contexts)
        {
            switch (context)
            {
                case StandardContext sc:
                    sc.Run();
                    continue;
                case YieldingContext yc:
                {
                    var handle = yc.Run();
                    while (handle.MoveNext())
                    {
                        yield return handle.Current;
                    }
                    break;
                }
            }
        }

        RunningScriptsList.Remove(this);
    }

    public TryGet<T> TryGetVariable<T>(VariableToken varToken) where T : Variable
    {
        if (_localVariables.TryGetValue((varToken.Prefix, varToken.Name), out var variable))
        {
            if (varToken.ValueType.CanHold(variable.BaseValue.Type))
            {
                if (variable is not T casted)
                {
                    return $"Variable '{varToken.RawRepr}' is not a {Variable.GetFriendlyName(typeof(T))}, but a {variable.FriendlyName} instead.";
                }

                return casted;
            }
        }
        
        if (VariableIndex.TryGetGlobalVariable(varToken.Prefix, varToken.Name, out var global))
        {
            if (varToken.ValueType.CanHold(global.BaseValue.Type))
            {
                if (global is T globalT)
                {
                    return globalT;
                }
            }
        }

        return $"Variable {varToken.RawRepr} doesn't exist or is inaccessible.";
    }

    public void AddLocalVariable(Variable variable)
    {
        if (!_localVariables.ContainsKey((variable.Prefix, variable.Name)) && VariableIndex.TryGetGlobalVariable(variable.Prefix, variable.Name, out _))
        {
            throw new CustomScriptRuntimeError(
                $"Tried to create a local variable '{variable}', but there already exists a global variable with the same name.");
        }

        Log.Debug($"Added variable {variable.Name} to script {Name}");
        _localVariables[(variable.Prefix, variable.Name)] = variable;
    }

    public void AddLocalVariables(params Variable[] variables)
    {
        foreach (var variable in variables)
        {
            AddLocalVariable(variable);
        }
    }

    public void RemoveLocalVariable(IVariableRepr variable)
    {
        _localVariables.Remove((variable.Prefix, variable.Name));
    }
}