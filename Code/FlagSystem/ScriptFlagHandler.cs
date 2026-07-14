using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.FlagSystem.Flags;
using SER.Code.Helpers;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.ScriptSystem.Structures;
using SER.Code.TokenSystem.Structures;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.FlagSystem;

public static class ScriptFlagHandler
{
    public static readonly Dictionary<ScriptName, List<Flag>> ScriptsFlags = [];
    public static Flag[] GetScriptFlags(ScriptName scriptName) => ScriptsFlags.TryGetValue(scriptName, out var flags)
        ? flags.ToArray()
        : [];
    
    internal static void Clear()
    {
        ScriptsFlags.Values.ForEachItem(script => script.ForEach(flag => flag.Unbind()));
        ScriptsFlags.Clear();
    }
    
    internal static Result RegisterScript(List<Line> scriptLinesWithFlags, ScriptName scriptName)
    {
        Flag? currentFlag = null;
        List<Flag> parsedFlags = [];

        foreach (var line in scriptLinesWithFlags)
        {
            var tokens = line.Tokens;
            var name = tokens.Skip(1).FirstOrDefault()?.RawRep;
            if (name is null)
            {
                RollBack(parsedFlags);
                return $"Line {line.LineNumber}: Name of the flag is missing.";
            }
            
            var args = tokens.Skip(2).Select(t => t.BestStaticTextRepr()).ToArray();
            var prefix = tokens.FirstOrDefault();
            
            var result = prefix switch
            {
                FlagToken => HandleFlag(name, args, scriptName, parsedFlags, ref currentFlag).Result,
                FlagArgumentToken => HandleFlagArgument(name, args, currentFlag),
                _ => throw new AndrzejFuckedUpException($"{prefix} not flag or flag arg")
            };
            
            if (result.HasErrored(out var error))
            {
                RollBack(parsedFlags);
                return $"Line {line.LineNumber}: {error}";
            }
        }

        ScriptsFlags[scriptName] = parsedFlags;
        parsedFlags.ForEach(flag => flag.OnParsingComplete());
        return true;
    }

    public static Result DoFlagsApproveExecution(Script scr, out bool mustReport)
    {
        mustReport = true;
        if (!ScriptsFlags.TryGetValue(scr.Name, out var scriptFlags))
        {
            return true;
        }

        foreach (var flag in scriptFlags)
        {
            if (flag.OnScriptRunning(scr, out mustReport).HasErrored(out var error))
            {
                Result rs = $"Flag '{flag.Name}' disallows script execution.";
                return rs + error;
            }
        }

        return true;
    }

    private static Result HandleFlagArgument(string argName, string[] arguments, Flag? currentFlag)
    {
        if (currentFlag is null)
        {
            return $"Tried to add argument '{argName}', but there is no valid flag above.";
        }

        var arg = currentFlag.Arguments.FirstOrDefault(arg => arg.Name == argName);
        if (string.IsNullOrEmpty(arg.Name))
        {
            return $"Flag {currentFlag.Name} does not accept a '{argName}' argument.";
        }

        if (arg.AddArgument(arguments).HasErrored(out var error))
        {
            return $"Error while handling flag argument '{argName}' for '{currentFlag.Name}': {error}";
        }

        return true;
    }

    private static TryGet<Flag> HandleFlag(
        string name,
        string[] arguments,
        ScriptName scriptName,
        List<Flag> parsedFlags,
        ref Flag? currentFlag)
    {
        var rs = $"Flag '{name}' failed when parsing.".AsError();
        
        if (Flag.TryGet(name, scriptName).HasErrored(out var getErr, out var flag))
        {
            return rs + getErr;
        }

        if (flag.InlineArgument.HasValue && flag.InlineArgument.Value.AddArgument(arguments).HasErrored(out var error))
        {
            return rs + error;
        }

        if (parsedFlags.Any(f => f.Name == flag.Name))
        {
            return rs + $"A '{flag.Name}' flag has been already registered once - one script cannot have two of the same flags.";
        }
        
        parsedFlags.Add(flag);
        currentFlag = flag;
        return flag;
    }

    private static void RollBack(IEnumerable<Flag> flags)
    {
        foreach (var flag in flags)
        {
            try
            {
                flag.Unbind();
            }
            catch (Exception exception)
            {
                Log.Error($"Failed to roll back flag '{flag.Name}': {exception.Message}");
            }
        }
    }
}











