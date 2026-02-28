using System.Collections.ObjectModel;
using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ContextSystem;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Contexts;
using SER.Code.ContextSystem.Extensions;
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
using SER.Code.VariableSystem.Variables;

namespace SER.Code.ScriptSystem;

public class Script
{
    private Line[] _lines = [];
    private Context[] _contexts = [];
    private bool? _isEventAllowed;
    
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

    public Profile? Profile { get; private set; }
    
    public RunReason RunReason { get; private set; }
    
    public uint CurrentLine { get; set; }
    
    public DateTime StartTime { get; private set; }

    public TimeSpan TimeRunning => StartTime == DateTime.MinValue ? TimeSpan.Zero : DateTime.Now - StartTime;

    private static readonly HashSet<Script> RunningScriptsList = [];
    public static readonly Script[] RunningScripts = RunningScriptsList.ToArray();
    
    private readonly HashSet<Variable> _localVariables = [];
    public Variable[] LocalVariables => _localVariables.ToArray();

    private readonly Dictionary<string, FuncStatement> _definedFunctions = [];
    public ReadOnlyDictionary<string, FuncStatement> DefinedFunctions => new(_definedFunctions);

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

    public static TryGet<Script> CreateByScriptName(string dirtyName, ScriptExecutor? executor)
    {
        var name = Path.GetFileNameWithoutExtension(dirtyName);
        if (ScriptName.TryInit(name).HasErrored(out var initError, out var scriptName))
        {
            return initError;       
        }

        return new Script
        {
            Name = scriptName,
            Content = File.ReadAllText(FileSystem.FileSystem.GetScriptPath(scriptName)),
            Executor = executor ?? ScriptExecutor.Get()
        };
    }
    
    public static Script CreateByVerifiedPath(string path, ScriptExecutor? executor) => new() 
    {
        Name = ScriptName.InitUnchecked(Path.GetFileNameWithoutExtension(path)),
        Content = File.ReadAllText(path),
        Executor = executor ?? ScriptExecutor.Get()
    };

    public static Script CreateAnonymous(string name, string content) => new()
    {
        Name = ScriptName.InitUnchecked(name),
        Content = content,
        Executor = ScriptExecutor.Get()
    };

    public static int StopAll()
    {
        var count = RunningScripts.Length;
        foreach (var script in new List<Script>(RunningScripts))
        {
            script.ExternalStop();
        }

        return count;
    }

    public void ExternalStop()
    {
        Killed = true;
    }
    
    public static int StopByName(string name)
    {
        var matches = new List<Script>(RunningScripts)
            .Where(scr => string.Equals(scr.Name, name, StringComparison.CurrentCultureIgnoreCase))
            .ToArray();
        
        matches.ForEachItem(scr => scr.ExternalStop());
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
    public static string? VerifyForExternalTool(string content)
    {
        if (MethodIndex.NameToMethodIndex.Count is 0)
        {
            MethodIndex.Initialize();
        }
        
        return CreateAnonymous("test", content).Compile().HasErrored(out var err) ? err : null;
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
        if (string.IsNullOrWhiteSpace(Content))
        {
            return null;
        }
        
        StartTime = DateTime.Now;
        RunReason = reason;
        Caller = caller;
        
        if (ScriptFlagHandler.DoFlagsApproveExecution(this).HasErrored(out var error))
        {
            Executor.Error(error, this);
            return null;
        }
        
        RunningScriptsList.Add(this);
        //Profile = new Profile(this);
        InternalExecute().Run(
            this,
            null,
            () => RunningScriptsList.Remove(this)
        );
        
        return _isEventAllowed;
    }

    public void SendControlMessage(ScriptControlMessage msg)
    {
        if (msg == ScriptControlMessage.EventNotAllowed)
        {
            _isEventAllowed = false;
        }
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

    private IEnumerator<float> InternalExecute()
    {
        if (Compile().HasErrored(out var err))
        {
            throw new ScriptCompileError(err);
        }
        
        foreach (var context in _contexts)
        {
            var handle = context.ExecuteBaseContext();
            while (handle.MoveNext())
            {
                yield return handle.Current;
            }
        }

        RunningScriptsList.Remove(this);
    }

    public TryGet<T> TryGetVariable<T>(VariableToken variable) where T : Variable
    {
        return TryGetVariable<T>(variable.Name);
    }

    public TryGet<T> TryGetVariable<T>(string name) where T : Variable
    {
        var variable = _localVariables.FirstOrDefault(v => v.Name == name);
        if (variable is not null)
        {
            if (variable is not T casted)
            {
                return $"Variable '{name}' is not a {Variable.GetFriendlyName(typeof(T))}, but a {variable.FriendlyName} instead.";
            }

            return casted;
        }
        
        var global = VariableIndex.GlobalVariables.FirstOrDefault(v => v.Name == name);
        if (global is T globalT)
        {
            return globalT;
        }

        return $"There is no variable called {name}.";
    }

    public void AddLocalVariable(Variable variable)
    {
        Variable.AssertNoVariableNameCollisions(variable, VariableIndex.GlobalVariables);
        
        Log.Debug($"Added variable {variable.Name} to script {Name}");
        RemoveLocalVariable(variable);
        _localVariables.Add(variable);
    }

    public void AddLocalVariables(params Variable[] variables)
    {
        foreach (var variable in variables)
        {
            AddLocalVariable(variable);
        }
    }

    public void RemoveLocalVariable(Variable variable)
    {
        _localVariables.RemoveWhere(lv => Variable.AreSyntacticallySame(lv, variable));
    }
}