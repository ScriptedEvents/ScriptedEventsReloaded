
using SER.Code.Helpers;
using SER.Code.Helpers.OldResultSystem;

namespace SER.Code.ScriptSystem.Structures;

public readonly record struct ScriptName
{
    private readonly string _value;
    
    private ScriptName(string value)
    {
        _value = value;
    }

    public Script? GetScriptWithAutomaticLog(ScriptExecutor? executor, bool assertCheck = true)
    {
        if (GetScript(executor, assertCheck).HasErrored(out var error, out var script))
        {
            Log.Error(error);
            return null;       
        }

        return script;
    }
    
    public OldTryGet<Script> GetScript(ScriptExecutor? executor, bool assertCheck = true)
    {
        if (assertCheck)
        {
            if (Assert(_value).HasErrored(out var error))
            {
                return error;
            }
        }
        
        executor ??= ScriptExecutor.Get();
        return Script.CreateByScriptName(this, executor);
    }

    public static ScriptName CreateUnsafe(string name) => new(name);

    public static OldTryGet<ScriptName> Create(string name)
    {
        if (Assert(name).HasErrored(out var error)) return error;

        return new ScriptName(name);
    }

    public static OldResult Assert(string name)
    {
        name = Path.GetFileNameWithoutExtension(name);
        if (!FileSystem.FileSystem.DoesScriptExistByName(name, out _))
        {
            return $"Script '{name}' does not exist in the SER folder or is inaccessible.";
        }

        return true;
    }

    public static implicit operator string(ScriptName scriptName)
    {
        return scriptName._value;
    }

    public static implicit operator ScriptName(Script script)
    {
        return script.Name;
    }

    public override string ToString()
    {
        return _value;
    }
    
    public override int GetHashCode() => _value.GetHashCode();
}