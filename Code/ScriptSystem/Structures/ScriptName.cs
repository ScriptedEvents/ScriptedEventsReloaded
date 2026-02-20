using SER.Code.Helpers.ResultSystem;

namespace SER.Code.ScriptSystem.Structures;

public readonly record struct ScriptName
{
    public readonly string Value;
    
    private ScriptName(string value)
    {
        Value = value;
    }

    public TryGet<Script> GetScript(ScriptExecutor? executor)
    {
        executor ??= ScriptExecutor.Get();
        return Script.CreateByScriptName(Value, executor);
    }

    public static ScriptName InitUnchecked(string name)
    {
        return new(name);
    }
    
    public static TryGet<ScriptName> TryInit(string name)
    {
        if (!FileSystem.FileSystem.DoesScriptExistByName(name, out _))
        {
            return $"Script '{name}' does not exist in the SER folder or is inaccessible.";
        }

        return new ScriptName(name);
    }

    public static implicit operator string(ScriptName scriptName)
    {
        return scriptName.Value;
    }

    public static implicit operator ScriptName(Script script)
    {
        return script.Name;
    }

    public override string ToString()
    {
        return Value;
    }
    
    public override int GetHashCode() => Value.GetHashCode();
}