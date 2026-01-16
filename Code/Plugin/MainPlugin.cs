using JetBrains.Annotations;
using LabApi.Features;
using LabApi.Features.Console;
using MEC;
using SER.Code.FlagSystem.Flags;
using SER.Code.Helpers;
using SER.Code.Helpers.Extensions;
using SER.Code.MethodSystem;
using SER.Code.MethodSystem.Methods.PlayerDataMethods;
using SER.Code.ScriptSystem;
using SER.Code.VariableSystem;
using EventHandler = SER.Code.EventSystem.EventHandler;
using Events = LabApi.Events.Handlers;

namespace SER.Code.Plugin;

[UsedImplicitly]
public class MainPlugin : LabApi.Loader.Features.Plugins.Plugin<Config>
{
    public override string Name => "SER";
    public override string Description => "The scripting language for SCP:SL.";
    public override string Author => "Elektryk_Andrzej";
    public override Version RequiredApiVersion => LabApiProperties.CurrentVersion;
    public override Version Version => new(0, 13, 0);

    public static string GitHubLink => "https://github.com/ScriptedEvents/ScriptedEventsReloaded";
    public static string DocsLink => "https://scriptedeventsreloaded.gitbook.io/docs/tutorial";
    public static string DiscordLink => "https://discord.gg/3j54zBnbbD";

    public static string HelpCommandName => "serhelp";
    public static MainPlugin Instance { get; private set; } = null!;

    public record struct Contributor(string Name, Contribution Contribution);

    [Flags]
    public enum Contribution
    {
        None = 0,
        LeadDeveloper = 1 << 1,
        Developer = 1 << 2,
        QualityAssurance = 1 << 3,
        Sponsor = 1 << 4,
        Betatester = 1 << 5,
        EarlyAdopter = 1 << 6,
        TechSupport = 1 << 7,
    }

    public static Contributor[] Contributors =>
    [
        new(Instance.Author, Contribution.LeadDeveloper),
        new("Whitty985playz", Contribution.QualityAssurance | Contribution.EarlyAdopter),
        new("Tosoks67", Contribution.Developer | Contribution.Betatester),
        new("Krzysiu Wojownik", Contribution.QualityAssurance | Contribution.Developer),
        new("Jraylor", Contribution.Sponsor),
        new("Luke", Contribution.Sponsor | Contribution.Betatester),
        new("Raging Tornado", Contribution.Betatester),
        new("Saskyc", Contribution.TechSupport)
    ];

    public override void Enable()
    {
        if (Config?.IsEnabled is false)
        {
            Logger.Info("Scripted Events Reloaded is disabled via config.");
            return;
        }
        
        Instance = this;
        
        Script.StopAll();
        EventHandler.Initialize();
        MethodIndex.Initialize();
        VariableIndex.Initialize();
        Flag.RegisterFlags();
        CommandEvents.Initialize();
        ExiledHelper.ExiledAwaiter().RunCoroutine();
        SendLogo();

        Events.ServerEvents.WaitingForPlayers += OnServerFullyInit;
        Events.ServerEvents.RoundRestarted += Disable;

        Timing.CallDelayed(1.5f, FileSystem.FileSystem.Initialize);
    }

    public override void Disable()
    {
        Script.StopAll();
        SetPlayerDataMethod.PlayerData.Clear();
    }

    private void OnServerFullyInit()
    {
        if (Config?.SendHelpMessageOnServerInitialization is false) return;

        Logger.Raw(
            $"""
             Thank you for using ### Scripted Events Reloaded ### by {Author}!

             Help command: {HelpCommandName}
             GitHub repository: {GitHubLink}
             Documentation: {DocsLink}
             Discord: {DiscordLink}
             """,
            ConsoleColor.Cyan
        );
    }

    private static void SendLogo()
    {
        Logger.Raw(
            """
            #####################################

              █████████  ██████████ ███████████  
             ███░░░░░███░░███░░░░░█░░███░░░░░███ 
            ░███    ░░░  ░███  █ ░  ░███    ░███ 
            ░░█████████  ░██████    ░██████████  
             ░░░░░░░░███ ░███░░█    ░███░░░░░███ 
             ███    ░███ ░███ ░   █ ░███    ░███ 
            ░░█████████  ██████████ █████   █████
             ░░░░░░░░░  ░░░░░░░░░░ ░░░░░   ░░░░░ 
             
            #####################################

            This project would not be possible without the help of:

            """ + Contributors
                .Select(c => $"> {c.Name} as {c
                    .Contribution
                    .GetFlags()
                    .Select(f => f.ToString().Spaceify())
                    .JoinStrings(", ")}"
                )
                .JoinStrings("\n"),
            ConsoleColor.Cyan
        );
    }
}