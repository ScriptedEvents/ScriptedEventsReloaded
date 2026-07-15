using System.ComponentModel;
#if EXILED 
using Exiled.API.Interfaces;
#endif

namespace SER.Code.Plugin;

#if EXILED
public class Config : IConfig
#else
public class Config
#endif
{
    public bool IsEnabled { get; set; } = true;

    [Description("This setting does not do anything lol")]
    public bool Debug { get; set; } = false;

    [Description("If true, SER will send a message to the server console when the plugin is enabled.")]
    public bool SendInitMessage { get; set; } = true;

    [Description("If true, SER will slow down scripts in order to prevent them from crashing the server.")]
    public bool SafeScripts { get; set; } = false;
    
    [Description("How many seconds a script file must remain unchanged before SER automatically reloads it. " +
                 "Running a script or using serreload still checks immediately.")]
    public float AutomaticScriptReloadDelay { get; set; } = 696969f;

    [Description("If you wish to remove ranks from players mentioned in the credits, message the developer.")]
    public int RankRemovalKey { get; set; } = 0;
}