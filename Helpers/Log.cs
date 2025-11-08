using System;
using LabApi.Features.Console;
using SER.ScriptSystem;

namespace SER.Helpers;

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

    public static void Warn(Script scr, object obj)
    {
        var ident = scr.CurrentLine == 0 ? "Compile warning" : $"Line {scr.CurrentLine}";
        Logger.Raw($"[Script '{scr.Name}'] [{ident}] {obj}", ConsoleColor.Yellow);
    }
    
    public static void Warn(string scrName, uint line, object obj)
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

    public static void D(string msg)
    {
        #if DEBUG
            Logger.Raw(msg, ConsoleColor.Cyan);
        #endif
    }
}