using System;
using System.Linq;
using JetBrains.Annotations;
using LabApi.Features;
using LabApi.Features.Console;
using MEC;
using SER.FlagSystem.Structures;
using SER.Helpers.Extensions;
using SER.MethodSystem;
using SER.MethodSystem.Methods.LiteralVariableMethods;
using SER.ScriptSystem;
using SER.VariableSystem;
using EventHandler = SER.EventSystem.EventHandler;
using Events = LabApi.Events.Handlers;

namespace SER.Plugin;

[UsedImplicitly]
public class MainPlugin : LabApi.Loader.Features.Plugins.Plugin<Config>
{
    public override string Name => "SER";
    public override string Description => "The scripting language for SCP:SL.";
    public override string Author => "Elektryk_Andrzej";
    public override Version RequiredApiVersion => LabApiProperties.CurrentVersion;
    public override Version Version => new(0, 8, 0);
    
    public static string GitHubLink => "https://github.com/ScriptedEvents/ScriptedEventsReloaded";
    public static string HelpCommandName => "serhelp";
    public static MainPlugin Instance { get; private set; } = null!;

    public record struct Contributor(string Name, Contribution Contribution);

    [Flags]
    public enum Contribution
    {
        None             = 0,
        LeadDeveloper    = 1 << 1,
        Developer        = 1 << 2,
        QualityAssurance = 1 << 3,
        Sponsor          = 1 << 4,
        Betatester       = 1 << 5,
        EarlyAdopter     = 1 << 6
    }

    public static Contributor[] Contributors => 
    [
        new(Instance.Author, Contribution.LeadDeveloper),
        new("Whitty985playz", Contribution.QualityAssurance | Contribution.EarlyAdopter),
        new("Jraylor", Contribution.Sponsor),
        new("Luke", Contribution.Betatester | Contribution.Sponsor),
        new("Krzysiu Wojownik", Contribution.QualityAssurance | Contribution.Developer),
        new("Raging Tornado", Contribution.Betatester)
    ];

    public override void Enable()
    {
        Instance = this;
        
        Script.StopAll();
        EventHandler.Initialize();
        MethodIndex.Initialize();
        VariableIndex.Initialize();
        Flag.RegisterFlags();
        CommandEvents.Initialize();
        SendLogo();
        
        Events.ServerEvents.WaitingForPlayers += OnServerFullyInit;
        Events.ServerEvents.RoundRestarted += Disable;
        
        Timing.CallDelayed(1.5f, FileSystem.Initialize);
    }

    public override void Disable()
    {
        Script.StopAll();
        SetPlayerDataMethod.PlayerData.Clear();
    }
    
    private void OnServerFullyInit()
    {
        Logger.Raw(
            $"""
             Thank you for using ### Scripted Events Reloaded ### by {Author}!

             Help command: {HelpCommandName}
             GitHub repository: {GitHubLink}
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