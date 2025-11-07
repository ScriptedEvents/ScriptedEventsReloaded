using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using MEC;
using SER.ContextSystem;
using SER.ContextSystem.BaseContexts;
using SER.ContextSystem.Extensions;
using SER.FlagSystem;
using SER.Helpers;
using SER.Helpers.Exceptions;
using SER.Helpers.Extensions;
using SER.Helpers.ResultSystem;
using SER.Plugin;
using SER.ScriptSystem.Structures;
using SER.TokenSystem;
using SER.TokenSystem.Structures;
using SER.TokenSystem.Tokens;
using SER.TokenSystem.Tokens.VariableTokens;
using SER.VariableSystem;
using SER.VariableSystem.Bases;
using SER.VariableSystem.Variables;

namespace SER.ScriptSystem;

public class Script
{
    public required string Name { get; init; }
    
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
    
    public uint CurrentLine { get; set; } = 0;
    
    public bool IsRunning => RunningScripts.Contains(this);

    private static readonly List<Script> _runningScripts = [];
    public static readonly ReadOnlyCollection<Script> RunningScripts = _runningScripts.AsReadOnly();


    private readonly HashSet<Variable> _variables = [];
    public ReadOnlyCollection<Variable> Variables => new(_variables.ToList());
    
    private CoroutineHandle _scriptCoroutine;
    private bool? _isEventAllowed;

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
        if (!FileSystem.DoesScriptExist(name, out var path))
        {
            return $"Script '{name}' does not exist in the SER folder or is inaccessible.";
        }

        return new Script
        {
            Name = name,
            Content = File.ReadAllText(path),
            Executor = executor ?? ScriptExecutor.Get()
        };
    }
    
    public static TryGet<Script> CreateByPath(string path, ScriptExecutor? executor)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        
        if (!FileSystem.DoesScriptExist(path))
        {
            return $"Script '{name}' does not exist in the SER folder or is inaccessible.";
        }

        return new Script
        {
            Name = name,
            Content = File.ReadAllText(path),
            Executor = executor ?? ScriptExecutor.Get()
        };
    }
    
    public static Script CreateByVerifiedPath(string path, ScriptExecutor? executor) => new() 
    {
        Name =  Path.GetFileNameWithoutExtension(path),
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
        
        _runningScripts.Add(this);
        _scriptCoroutine = InternalExecute().Run(this, _ => _scriptCoroutine.Kill());
        return _isEventAllowed;
    }

    public void Stop(bool silent = false)
    {
        _runningScripts.Remove(this);
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
        if (Tokenizer.GetInfoFromMultipleLines(Content).HasErrored(out var err, out var info))
        {
            return "Defining script lines failed." + err;
        }
        
        Log.Debug($"Script {Name} defines {info.Length} lines");
        Lines = info;
        return true;
    }
    
    public Result SliceLines()
    {
        foreach (var line in Lines)
        {
            if (Tokenizer.SliceLine(line).HasErrored(out var error))
            {
                Result mainErr = $"Processing line {line.LineNumber} has failed.";
                return mainErr + error;
            }
        }
        
        Log.Debug($"Script {Name} sliced {Lines.Length} lines into {Lines.Sum(l => l.Slices.Length)} slices");
        return true;
    }

    public Result TokenizeLines()
    {
        foreach (var line in Lines)
        {
            if (Tokenizer.TokenizeLine(line, this).HasErrored(out var error))
            {
                return error;
            }
        }

        Log.Debug($"Script {Name} tokenized {Lines.Length} lines into {Lines.Sum(l => l.Tokens.Length)} tokens");
        return true;
    }
    
    private Result ContextLines()
    {
        if (Contexter.ContextLines(Lines, this).HasErrored(out var err, out var contexts))
        {
            return err;
        }
        
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
            throw new ScriptRuntimeError(err);
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

        _runningScripts.Remove(this);
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