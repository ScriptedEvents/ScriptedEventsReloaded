using System.Diagnostics;
using System.Text;
using LabApi.Features.Console;
using SER.Code.ScriptSystem;

namespace SER.Code.Helpers;

public static class Log
{
    public static void Debug<T>(T obj)
    {
        #if DEBUG
            Logger.Raw($"Debug: {obj!.ToString()}", ConsoleColor.Gray);
        #endif
    }

    public static void Signal(object o)
    {
        Logger.Raw(o.ToString(), ConsoleColor.Magenta);
    }

    public static void Warn(object o)
    {
        Logger.Raw(o.ToString(), ConsoleColor.Yellow);
    }

    public static void Error(object o)
    {
        Logger.Raw(o.ToString(), ConsoleColor.Red);
    }

    public static void ScriptWarn(Script scr, object obj)
    {
        var ident = scr.CurrentLine == 0 ? "Compile warning" : $"Line {scr.CurrentLine}";
        Logger.Raw($"[Script '{scr.Name}'] [{ident}] {obj}", ConsoleColor.Yellow);
    }
    
    public static void ScriptWarn(string scrName, uint line, object obj)
    {
        Logger.Raw($"[Script '{scrName}'] [Line {line}] {obj}", ConsoleColor.Yellow);
    }
    
    public static void RuntimeError(string scrName, uint line, string msg)
    {
        Logger.Raw($"[Script '{scrName}'] [Line {line}] {msg}", ConsoleColor.Red);
    }
    
    public static void CompileError(string scrName, string msg)
    {
        Logger.Raw($"[Script '{scrName}'] [Compile error] {msg}", ConsoleColor.Red);
    }
    
    public static void CompileError(string scrName, uint line, string msg)
    {
        Logger.Raw($"[Script '{scrName}'] [Compile error] [Line {line}] {msg}", ConsoleColor.Red);
    }
    
    public static void D(string msg)
    {
        #if DEBUG
            Logger.Raw(msg, ConsoleColor.Cyan);
        #endif
    }
    
    public static string GetStackTrace(int depth = 10)
    {
        StackTrace stackTrace = new StackTrace(true);

        StringBuilder sb = new("");
        foreach (var stackFrame in stackTrace.GetFrames()!.Take(depth))
        {
            sb.AppendLine($"-> {stackFrame.GetMethod().Name}");
        }
        
        return sb.ToString();
    }
}

