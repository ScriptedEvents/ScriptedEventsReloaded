using System.Reflection;
using LabApi.Features.Console;
using SER.Code.Extensions;
using SER.Code.FlagSystem.Structures;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.ScriptSystem.Structures;

namespace SER.Code.FlagSystem.Flags;

public abstract class Flag
{
    public abstract string Description { get; }

    public readonly record struct Argument(
        string Name, 
        string Description, 
        Func<string[], Result> Handler, 
        bool IsRequired,
        string Example
    ) {
        public Result AddArgument(string[] values) => Handler(values);
    }

    public abstract Argument? InlineArgument { get; }

    public abstract Argument[] Arguments { get; }

    public virtual void OnParsingComplete()
    {
    }

    public virtual Result Bind()
    {
        OnParsingComplete();
        return true;
    }
    
    public abstract void Unbind();
    
    public virtual Result OnScriptRunning(Script scr, out bool mustReport)
    {
        mustReport = true;
        if (this is IMajorBehaviorFlag)
        {
            if (ScriptFlagHandler.ScriptsFlags[scr].Without(this)
                    .FirstOrDefault(f => f is IMajorBehaviorFlag) is {} conflictingFlag)
            {
                return $"Flag '{Name}' and '{conflictingFlag.Name}' cannot be used in the same script.";
            }
        }
        
        return true;
    }

    // when this fucker is set to null, it still compiles but the plugin will just fucking explode
    // even better, the errors will be pointing to a random line in some child class
    // https://tenor.com/view/alucore-gif-21193899
    protected ScriptName ScriptName { get; set; } = default;

    public string Name { get; set; } = null!;
    
    public static Dictionary<string, Type> FlagInfos = [];

    public static void RegisterFlags(Assembly? ass = null)
    {
        ass ??= Assembly.GetExecutingAssembly();
        FlagInfos = GetRegisteredFlags(ass);
    }

    // ReSharper disable once UnusedMember.Global
    public static void RegisterFlagsAsExternalPlugin()
    {
        Logger.Info($"Registering flags from '{Assembly.GetCallingAssembly().GetName().Name}' plugin.");
        var flags = GetRegisteredFlags(Assembly.GetCallingAssembly());
        FlagInfos = FlagInfos.Concat(flags).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    private static Dictionary<string, Type> GetRegisteredFlags(Assembly? ass = null)
    {
        ass ??= Assembly.GetExecutingAssembly();
        return ass.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(Flag).IsAssignableFrom(t))
            .ToDictionary(t => t.Name.Replace("Flag", ""), t => t);
    }

    public static TryGet<Flag> TryGet(string flagName, ScriptName scriptName) 
    {
        if (!FlagInfos.TryGetValue(flagName, out var type))
        {
            return $"Flag '{flagName}' is not a valid flag.";
        }
        
        var flag = type.CreateInstance<Flag>();

        flag.ScriptName = scriptName;
        flag.Name = flagName;
        return flag;
    }
}
