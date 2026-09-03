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

#if EXILED
    [Description("Enables debug logging provided by EXILED.")]
    public bool Debug { get; set; } = false;
#endif

    [Description("If true, SER sends a concise status message when the plugin is ready.")]
    public bool SendInitMessage { get; set; } = true;

    [Description(
        "Slows scripts down slightly to help stop them from freezing the server. " +
        "Keep this enabled unless you have checked every active script."
    )]
    public bool SafeScripts { get; set; } = true;

    [Description("Prints the large SER logo and contributor list when the plugin is enabled.")]
    public bool SendLogo { get; set; } = false;

    [Description("Shows a temporary SER contributor badge to recognized contributors without a server rank.")]
    public bool ShowContributorBadges { get; set; } = false;

    [Description(
        "Maximum time in seconds that an HTTP, Discord, or IP-information request may run before it is cancelled."
    )]
    public int NetworkRequestTimeoutSeconds { get; set; } = 15;

    [Description(
        "Maximum response body size in bytes accepted by HTTP, Discord, and IP-information requests."
    )]
    public int MaxNetworkResponseBytes { get; set; } = 1_048_576;
}
