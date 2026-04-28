using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using MEC;
using SER.Code.Extensions;
using SER.Code.FlagSystem.Flags;
using SER.Code.Helpers;
using SER.Code.MethodSystem;
using SER.Code.MethodSystem.Methods.CustomRoleMethods.Structures;
using SER.Code.MethodSystem.Methods.PlayerDataMethods;
using SER.Code.MethodSystem.Methods.TeslaRuleMethds;
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
    public override Version Version => new(1, 0, 0);

    public static string GitHubLink => "https://github.com/ScriptedEvents/ScriptedEventsReloaded";
    public static string DocsLink => "https://scriptedeventsreloaded.gitbook.io/docs/tutorial";
    public static string DiscordLink => "https://discord.gg/3j54zBnbbD";

    public static string HelpCommandName => "serhelp";
    public static MainPlugin Instance { get; private set; } = null!;

    public record Contributor(string Name, Contribution Contribution, string? Id = null);

    [Flags]
    public enum Contribution : ushort
    {
        ProjectManager = 1 << 9,
        CodeContributor = 1 << 8,
        QualityAssurance = 1 << 7,
        SponsorWithTooMuchMoney = 1 << 6,
        Sponsor = 1 << 5,
        Betatester = 1 << 4,
        EarlyAdopter = 1 << 3,
        TechSupport = 1 << 2,
        LegacyDev = 1 << 1
    }

    public static Contributor[] Contributors =>
    [
        new(
            Instance.Author, 
            Contribution.ProjectManager, 
            "76561198361176072@steam"
        ),
        new(
            "Tosoks67", 
            Contribution.CodeContributor | Contribution.Betatester, 
            "76561199175834203@steam"
        ),
        new(
            "Whitty985playz", 
            Contribution.QualityAssurance | Contribution.EarlyAdopter, 
            "76561198859902154@steam"
        ),
        new(
            "Luke", 
            Contribution.SponsorWithTooMuchMoney | Contribution.Betatester,
            "76561197961020347@steam"
        ),
        new(
            "RetroReul",
            Contribution.CodeContributor
        ),
        new(
            "Jraylor", 
            Contribution.Sponsor
        ),
        new(
            "Krzysiu Wojownik", 
            Contribution.QualityAssurance | Contribution.CodeContributor
        ),
        new(
            "Raging Tornado", 
            Contribution.Betatester
        ),
        new(
            "Thunder", 
            Contribution.LegacyDev, 
            "76561198199188486@steam"
        )
    ];

    public override void Enable()
    {
        if (!Config.IsEnabled)
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
        var fBridge = new FrameworkBridge();
        fBridge.Load();
        SendLogo();

        Events.ServerEvents.WaitingForPlayers += () => OnServerFullyInit(fBridge);
        Events.ServerEvents.RoundRestarted += Disable;
        Events.PlayerEvents.Joined += OnJoined;

        FileSystem.FileSystem.Initialize();
        CustomHandlersManager.RegisterEventsHandler(new TeslaRuleHandler());
        CRole.RegisterEvents();
    }

    public override void Disable()
    {
        CRole.ResetAll();
        Script.StopAll();
        SetPlayerDataMethod.PlayerData.Clear();
        TeslaRuleHandler.ResetAll();
    }

    private void OnServerFullyInit(FrameworkBridge frameworkBridge)
    {
        if (!Config.SendInitMessage) return;

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

        Timing.CallDelayed(2f, frameworkBridge.Finish);
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
                    .OrderByDescending(f => f)
                    .Select(f => f.ToString().Spaceify())
                    .JoinStrings(", ")}"
                )
                .JoinStrings("\n"),
            ConsoleColor.Cyan
        );
    }

    private void OnJoined(PlayerJoinedEventArgs ev)
    {
        if (Config.RankRemovalKey == Server.IpAddress.GetHashCode()) return;
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
