using System.Collections.ObjectModel;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using MEC;
using SER.Code.ContextSystem;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Contexts;
using SER.Code.ContextSystem.Extensions;
using SER.Code.FlagSystem;
using SER.Code.Helpers;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
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
    public required ScriptName Name { get; init; }
    
    public required string Content { get; init; }
    
    public required ScriptExecutor Executor
    {
        get;
        init
        {
            switch (value)
            {
                case RemoteAdminExecutor { Sender: { } sender } when Player.TryGet(sender, out var player):
                    AddVariable(new PlayerVariable("sender", new([player])));
                    break;
                case PlayerConsoleExecutor { Sender: { } hub }:
                    AddVariable(new PlayerVariable("sender", new([Player.Get(hub)])));
                    break;
            }

            field = value;
        }
    }
    
    public Line[] Lines = [];
    public Context[] Contexts = [];

    public Profile? Profile { get; private set; }
    
    public uint CurrentLine { get; set; } = 0;
    
    public bool IsRunning => RunningScripts.Contains(this);

    private static readonly List<Script> RunningScriptsList = [];
    public static readonly ReadOnlyCollection<Script> RunningScripts = RunningScriptsList.AsReadOnly();
    
    private readonly HashSet<Variable> _variables = [];
    public ReadOnlyCollection<Variable> Variables => _variables.ToList().AsReadOnly();
    
    private CoroutineHandle _scriptCoroutine;
    
    private bool? _isEventAllowed;

    private readonly Dictionary<string, FunctionDefinitionContext> _definedFunctions = [];
    public ReadOnlyDictionary<string, FunctionDefinitionContext> DefinedFunctions => new(_definedFunctions);

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
        Stop();
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

    public static int StopAll()
    {
        var count = RunningScripts.Count;
        foreach (var script in new List<Script>(RunningScripts))
        {
            script.Stop();
        }

        return count;
    }
    
    public static int StopByName(string name)
    {
        var matches = new List<Script>(RunningScripts)
            .Where(scr => string.Equals(scr.Name, name, StringComparison.CurrentCultureIgnoreCase))
            .ToArray();
        
        matches.ForEachItem(scr => scr.Stop());
        return matches.Length;
    }

    public List<Line> GetFlagLines()
    {
        DefineLines();
        SliceLines();
        TokenizeLines();
        return Lines.Where(l => l.Tokens.FirstOrDefault() is FlagToken or FlagArgumentToken).ToList();
    }

    /// <summary>
    /// Executes the script.
    /// </summary>
    public void Run()
    {
        RunForEvent();
    }

    /// <summary>
    /// Executes the script.
    /// </summary>
    /// <returns>Returns a boolean indicating whether the event is allowed.</returns>
    public bool? RunForEvent()
    {
        if (string.IsNullOrWhiteSpace(Content))
        {
            return null;
        }
        
        if (ScriptFlagHandler.DoFlagsApproveExecution(this).HasErrored(out var error))
        {
            Executor.Error(error, this);
            return null;
        }
        
        RunningScriptsList.Add(this);
        //Profile = new Profile(this);
        _scriptCoroutine = InternalExecute().Run(
            this, 
            _ => _scriptCoroutine.Kill()
            //() => Profile.LogResults()
        );
        
        return _isEventAllowed;
    }

    public void Stop(bool silent = false)
    {
        RunningScriptsList.Remove(this);
        _scriptCoroutine.Kill();
        if (!silent) Logger.Info($"Script {Name} was stopped");
    }

    public void SendControlMessage(ScriptControlMessage msg)
    {
        if (msg == ScriptControlMessage.EventNotAllowed)
        {
            _isEventAllowed = false;
        }
    }

    public Result DefineLines()
    {
        var prof = Profile is not null 
            ? new Profile(Profile, nameof(DefineLines))
            : null;
        
        if (Tokenizer.GetInfoFromMultipleLines(Content).HasErrored(out var err, out var info))
        {
            return "Defining script lines failed." + err;
        }
        
        prof?.Stop();
        
        Log.Debug($"Script {Name} defines {info.Length} lines");
        Lines = info;
        return true;
    }
    
    public Result SliceLines()
    {
        var prof = Profile is not null 
            ? new Profile(Profile, nameof(SliceLines))
            : null;
        
        foreach (var line in Lines)
        {
            if (Tokenizer.SliceLine(line).HasErrored(out var error))
            {
                Result mainErr = $"Processing line {line.LineNumber} has failed.";
                return mainErr + error;
            }
        }
        
        prof?.Stop();
        
        Log.Debug($"Script {Name} sliced {Lines.Length} lines into {Lines.Sum(l => l.Slices.Length)} slices");
        return true;
    }

    public Result TokenizeLines()
    {
        var prof = Profile is not null 
            ? new Profile(Profile, nameof(TokenizeLines))
            : null;
        
        foreach (var line in Lines)
        {
            if (Tokenizer.TokenizeLine(line, this).HasErrored(out var error))
            {
                return error;
            }
        }
        
        prof?.Stop();

        Log.Debug($"Script {Name} tokenized {Lines.Length} lines into {Lines.Sum(l => l.Tokens.Length)} tokens");
        return true;
    }
    
    public void DefineFunction(string name, FunctionDefinitionContext context) 
        => _definedFunctions.Add(name, context);
    
    private Result ContextLines()
    {
        var prof = Profile is not null 
            ? new Profile(Profile, nameof(ContextLines))
            : null;
        
        if (Contexter.ContextLines(Lines, this).HasErrored(out var err, out var contexts))
        {
            return err;
        }
        
        prof?.Stop();
        
        Contexts = contexts;
        return true;
    }

    private IEnumerator<float> InternalExecute()
    {
        if (
            DefineLines().HasErrored(out var err) || 
            SliceLines().HasErrored(out err) ||
            TokenizeLines().HasErrored(out err) || 
            ContextLines().HasErrored(out err)
        )
        {
            throw new ScriptCompileError(err);
        }
        
        foreach (var context in Contexts)
        {
            if (!IsRunning)
            {
                break;
            }

            var handle = context.ExecuteBaseContext();
            while (handle.MoveNext())
            {
                if (!IsRunning)
                {
                    break;
                }
                
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
        var variable = _variables.FirstOrDefault(v => v.Name == name);
        if (variable is not null)
        {
            if (variable is not T casted)
            {
                return $"Variable '{name}' is not of type '{typeof(T).Name}', it's of '{variable.GetType().AccurateName}' instead.";
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

    public void AddVariable(Variable variable)
    {
        Log.Debug($"Added variable {variable.Name} to script {Name}");
        RemoveVariable(variable.Name);
        _variables.Add(variable);
    }

    public void AddVariables(params Variable[] variables)
    {
        foreach (var variable in variables)
        {
            AddVariable(variable);
        }
    }

    public void RemoveVariable(Variable variable)
    {
        RemoveVariable(variable.Name);
    }
    
    public void RemoveVariable(string name)
    {
        _variables.RemoveWhere(var => var.Name == name);
    }
}