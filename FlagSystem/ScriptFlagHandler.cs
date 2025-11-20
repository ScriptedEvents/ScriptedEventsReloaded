using SER.FlagSystem.Flags;
using SER.Helpers;
using SER.Helpers.Exceptions;
using SER.Helpers.Extensions;
using SER.Helpers.ResultSystem;
using SER.ScriptSystem;
using SER.ScriptSystem.Structures;
using SER.TokenSystem.Structures;
using SER.TokenSystem.Tokens;
using EventHandler = SER.EventSystem.EventHandler;

namespace SER.FlagSystem;

public static class ScriptFlagHandler
{
    private static readonly Dictionary<ScriptName, List<Flag>> ScriptsFlags = [];
    public static Flag[] GetScriptFlags(ScriptName scriptName) => ScriptsFlags[scriptName].ToArray();
    
    private static Flag? _currentFlag;

    internal static void Clear()
    {
        _currentFlag = null;
        ScriptsFlags.Values.ForEachItem(script => script.ForEach(flag => flag.Unbind()));
        ScriptsFlags.Clear();
        EventHandler.EventClear();
    }
    
    internal static void RegisterScript(List<Line> scriptLinesWithFlags, ScriptName scriptName)
    {
        foreach (var line in scriptLinesWithFlags)
        {
            var tokens = line.Tokens;
            var name = tokens.Skip(1).FirstOrDefault()?.RawRep;
            if (name is null)
            {
                Log.RuntimeError(scriptName, line.LineNumber, "Name of the flag is missing.");
                return;
            }
            
            var args = tokens.Skip(2).Select(t => t.GetBestTextRepresentation(null)).ToArray();
            var prefix = tokens.FirstOrDefault();
            
            var result = prefix switch
            {
                FlagToken => HandleFlag(name, args, scriptName).Result,
                FlagArgumentToken => HandleFlagArgument(name, args),
                _ => throw new AndrzejFuckedUpException($"{prefix} not flag or flag arg")
            };
            
            if (result.HasErrored(out var error))
            {
                Log.RuntimeError(scriptName, line.LineNumber, error);
                return;
            }
        }
        
        _currentFlag?.OnParsingComplete();
        _currentFlag = null;
    }

    public static Result DoFlagsApproveExecution(Script scr)
    {
        if (!ScriptsFlags.TryGetValue(scr.Name, out var scriptFlags))
        {
            return true;
        }

        foreach (var flag in scriptFlags)
        {
            if (flag.OnScriptRunning(scr).HasErrored(out var error))
            {
                Result rs = $"Script '{scr.Name}' cannot run due to flag '{flag.Name}'.";
                return rs + error;
            }
        }

        return true;
    }

    private static Result HandleFlagArgument(string argName, string[] arguments)
    {
        if (_currentFlag is null)
        {
            return $"Tried to add argument '{argName}', but there is no valid flag above.";
        }

        var arg = _currentFlag.Arguments.FirstOrDefault(arg => arg.Name == argName);
        if (string.IsNullOrEmpty(arg.Name))
        {
            return $"Flag {_currentFlag.Name} does not accept a '{argName}' argument.";
        }

        if (arg.AddArgument(arguments).HasErrored(out var error))
        {
            return $"Error while handling flag argument '{argName}' for '{_currentFlag.Name}': {error}";
        }

        return true;
    }

    private static TryGet<Flag> HandleFlag(string name, string[] arguments, ScriptName scriptName)
    {
        _currentFlag?.OnParsingComplete();
        Result rs = $"Flag '{name}' failed when parsing.";
        
        if (Flag.TryGet(name, scriptName).HasErrored(out var getErr, out var flag))
        {
            return rs + getErr;
        }

        if (flag.InlineArgument.HasValue && flag.InlineArgument.Value.AddArgument(arguments).HasErrored(out var error))
        {
            return rs + error;
        }
        
        ScriptsFlags.AddOrInitListWithKey(scriptName, flag);
        return _currentFlag = flag;
    }
}











