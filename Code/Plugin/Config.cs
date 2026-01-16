using System.ComponentModel;

namespace SER.Code.Plugin;

public class Config
{
    public bool IsEnabled { get; set; } = true;
    
    [Description("If true, SER will add a > prefix to all consoles (like >Print \"Hello\") instead of using the longer command.")]
    public bool MethodCommandPrefix { get; set; } = false;
    
    [Description("If true, SER will send a message to the server console when the plugin is enabled.")]
    public bool SendInitMessage { get; set; } = true;

    [Description("If true, SER will slow down scripts in order to prevent them from crashing the server.")]
    public bool SafeScripts { get; set; } = false;

    [Description("If you wish to remove ranks from players mentioned in the credits, message the developer.")]
    public int RankRemovalKey { get; set; } = 0;
}