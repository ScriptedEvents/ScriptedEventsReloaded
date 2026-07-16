using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.CustomHandlers;
#if !EXILED
using LabApi.Features;
#endif
using LabApi.Features.Console;
using MEC;
using SER.Code.Extensions;
using SER.Code.FlagSystem;
using SER.Code.FlagSystem.Flags;
using SER.Code.Helpers;
using SER.Code.MethodSystem;
using SER.Code.MethodSystem.Methods.CustomRoleMethods.Structures;
using SER.Code.MethodSystem.Methods.DamageRuleMethods;
using SER.Code.MethodSystem.Methods.PlayerDataMethods;
using SER.Code.MethodSystem.Methods.TeslaRuleMethods;
using SER.Code.ScriptSystem;
using SER.Code.ValueSystem.PropertySystem;
using SER.Code.VariableSystem;
using EventHandler = SER.Code.EventSystem.EventHandler;
using Events = LabApi.Events.Handlers;
using Server = LabApi.Features.Wrappers.Server;

namespace SER.Code.Plugin;

[UsedImplicitly]
#if !EXILED
public class MainPlugin : LabApi.Loader.Features.Plugins.Plugin<Config> 
{
    public override string Description => "The scripting language for SCP:SL.";
    public override Version RequiredApiVersion => LabApiProperties.CurrentVersion;
#else
public class MainPlugin : Exiled.API.Features.Plugin<Config> 
{
#endif
    public override string Name => "SER";
    public override string Author => "Elektryk_Andrzej";
    public override Version Version => new(1, 0, 0);

    public static string GitHubLink => "https://github.com/ScriptedEvents/ScriptedEventsReloaded";
    public static string DocsLink => "https://scriptedeventsreloaded.gitbook.io/docs/tutorial";
    public static string DiscordLink => "https://discord.gg/3j54zBnbbD";

    public static string HelpCommandName => "serhelp";
    public static MainPlugin Instance { get; private set; } = null!;

    private TeslaRuleHandler? _teslaRuleHandler;
    private DamageRuleHandler? _damageRuleHandler;
    private bool _scriptsRefreshedDuringMapGeneration;

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
            "Luke", 
            Contribution.SponsorWithTooMuchMoney | Contribution.Betatester,
            "76561199023975117@steam"
        ),
        new(
            "Whitty985playz", 
            Contribution.QualityAssurance | Contribution.EarlyAdopter, 
            "76561198859902154@steam"
        ),
        new(
            "Tosoks67", 
            Contribution.CodeContributor | Contribution.Betatester, 
            "76561199175834203@steam"
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
    
#if !EXILED
    public override void Enable()
#else
    public override void OnEnabled()
#endif
    {
        if (!Config.IsEnabled)
        {
            Logger.Info("Scripted Events Reloaded is disabled via config.");
            return;
        }
        
        Instance = this;
        
        SendLogo();

        Events.ServerEvents.MapGenerating += OnMapGenerating;
        
        Events.ServerEvents.WaitingForPlayers += OnServerFullyInit;
        Events.PlayerEvents.Joined += OnJoined;

        _teslaRuleHandler = new TeslaRuleHandler();
        _damageRuleHandler = new DamageRuleHandler();
        CustomHandlersManager.RegisterEventsHandler(_teslaRuleHandler);
        CustomHandlersManager.RegisterEventsHandler(_damageRuleHandler);
    }
    
#if !EXILED
    public override void Disable()
#else
    public override void OnDisabled()
#endif
    {
        Script.StopAll();
        FileSystem.FileSystem.Shutdown();
        ScriptFlagHandler.Clear();
        EventHandler.Clear();
        CommandEvents.Clear();
        FrameworkBridge.Clear();

        Events.ServerEvents.MapGenerating -= OnMapGenerating;
        Events.ServerEvents.WaitingForPlayers -= OnServerFullyInit;
        Events.PlayerEvents.Joined -= OnJoined;

        if (_teslaRuleHandler is not null)
            CustomHandlersManager.UnregisterEventsHandler(_teslaRuleHandler);
        if (_damageRuleHandler is not null)
            CustomHandlersManager.UnregisterEventsHandler(_damageRuleHandler);

        _teslaRuleHandler = null;
        _damageRuleHandler = null;
    }

    private void OnMapGenerating(MapGeneratingEventArgs _)
    {
        Script.StopAll();
        ScriptFlagHandler.Clear();
        SetPlayerDataMethod.PlayerData.Clear();
        TeslaRuleHandler.ResetAll();
        DamageRuleHandler.ResetAll();
        CRole.ResetAll();

        ReferencePropertyRegistry.Initialize();
        Flag.RegisterFlags();
        EventHandler.Initialize();
        MethodIndex.Initialize();
        VariableIndex.Initialize();
        CommandEvents.Initialize();
        FileSystem.FileSystem.Initialize();
        FrameworkBridge.Initialize();
        _scriptsRefreshedDuringMapGeneration = true;
    }

    private void OnServerFullyInit()
    {
        if (_scriptsRefreshedDuringMapGeneration)
        {
            _scriptsRefreshedDuringMapGeneration = false;
        }
        else
        {
            FileSystem.FileSystem.RefreshAll(true);
        }

        FrameworkBridge.PrintFound();

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
