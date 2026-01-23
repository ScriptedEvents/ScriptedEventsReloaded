using JetBrains.Annotations;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
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

    public record Contributor(string Name, Contribution Contribution, string? Id = null);

    [Flags]
    public enum Contribution : ushort
    {
        LeadDeveloper = 1 << 8,
        Developer = 1 << 7,
        QualityAssurance = 1 << 6,
        Sponsor = 1 << 5,
        Betatester = 1 << 4,
        EarlyAdopter = 1 << 3,
        TechSupport = 1 << 2,
        LegacyDeveloper = 1 << 1
    }

    public static Contributor[] Contributors =>
    [
        new(
            Instance.Author, 
            Contribution.LeadDeveloper, 
            "76561198361176072@steam"
        ),
        new(
            "Whitty985playz", 
            Contribution.QualityAssurance | Contribution.EarlyAdopter, 
            "76561198859902154@steam"
        ),
        new(
            "Tosoks67", 
            Contribution.Developer | Contribution.Betatester, 
            "76561199175834203@steam"
        ),
        new(
            "Krzysiu Wojownik", 
            Contribution.QualityAssurance | Contribution.Developer
        ),
        new(
            "Jraylor", 
            Contribution.Sponsor
        ),
        new(
            "Luke", 
            Contribution.Sponsor | Contribution.Betatester,
            "76561197961020347@steam"
        ),
        new(
            "Raging Tornado", 
            Contribution.Betatester
        ),
        new(
            "Saskyc", 
            Contribution.TechSupport
        ),
        new(
            "Thunder", 
            Contribution.LegacyDeveloper, 
            "76561198199188486@steam"
        )
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
        Events.PlayerEvents.Joined += OnJoined;

        Timing.CallDelayed(1.5f, FileSystem.FileSystem.Initialize);
    }

    public override void Disable()
    {
        Script.StopAll();
        SetPlayerDataMethod.PlayerData.Clear();
    }

    private void OnServerFullyInit()
    {
        if (Config?.SendInitMessage is false) return;

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

    private void OnJoined(PlayerJoinedEventArgs ev)
    {
        if (Config?.RankRemovalKey is { } key && Server.IpAddress.GetHashCode() == key) return;
        if (ev.Player is not { } plr) return;
        
        Timing.CallDelayed(3f, () =>
        {
            if (plr.UserGroup is not null) return;
            if (Contributors.FirstOrDefault(c => c.Id == plr.UserId && c.Id is not null) is not { } info) return;
            
            plr.GroupColor = "aqua";
            plr.GroupName = $"* SER {info
                .Contribution
                .GetFlags()
                .OrderByDescending(f => f)
                .First()
                .ToString()
                .Spaceify()} *";
        });
    }
}
