using System.Diagnostics;
using LabApi.Features.Console;
using SER.Code.Extensions;
using SER.Code.ScriptSystem;

namespace SER.Code.Helpers;

public class Profile
{
    public readonly Script Script;
    public string? Type { get; }
    private readonly Stopwatch _time = new();
    protected readonly List<Profile> Children = [];

    public Profile(Profile parent, string type) : this(parent.Script)
    {
        parent.Children.Add(this);
        Type = type;
    }
    
    public Profile(Profile parent, Type type) : this(parent.Script)
    {
        parent.Children.Add(this);
        Type = type.FullName;
    }

    public Profile(Script scr)
    {
        _time.Start();
        Script = scr;
    }

    public Profile Stop()
    {
        _time.Stop();
        return this;
    }

    public void LogResults()
    {
        if (_time.IsRunning) _time.Stop();
        
        Logger.Info(
            $"This profile took {_time.ElapsedMilliseconds}ms to run."
            + Children.Select(c => $"\n\t> {c.GetFormattedResult()}").JoinStrings("")
        );
    }

    protected string GetFormattedResult()
    {
        if (Type is null) return string.Empty;
        
        return $"{Type}: {_time.ElapsedMilliseconds}ms";
    }
}