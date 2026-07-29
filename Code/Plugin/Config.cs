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
        "Inserts frame yields while scripts execute, reducing the risk that a tight script stalls the server. " +
        "Disable this only after reviewing every active script."
    )]
    public bool SafeScripts { get; set; } = true;

    [Description("Prints the large SER logo and contributor list when the plugin is enabled.")]
    public bool SendLogo { get; set; } = false;

    [Description("Shows a temporary SER contributor badge to recognized contributors without a server rank.")]
    public bool ShowContributorBadges { get; set; } = false;
}
