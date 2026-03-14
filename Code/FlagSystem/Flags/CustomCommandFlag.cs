using CommandSystem;
using JetBrains.Annotations;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using RemoteAdmin;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.ScriptSystem.Structures;
using SER.Code.TokenSystem;
using SER.Code.TokenSystem.Tokens.ValueTokens;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Bases;
using SER.Code.VariableSystem.Variables;
using Console = GameCore.Console;

namespace SER.Code.FlagSystem.Flags;

[UsedImplicitly]
public class CustomCommandFlag : Flag
{
    public override string Description =>
        """
        Creates a command and binds it to the script. When the command is ran, it executes the script. 

        Injects the following variables into the script:
        > @sender (the player who ran the command, NOT added when command is ran by the server console)
        > *command (reference to this 'CustomCommand', used for resetting cooldown)
        """;
    
    public override Argument? InlineArgument => new(
        "command name",
        "The name of the command to create",
        inlineArgs =>
        {
            switch (inlineArgs.Length)
            {
                case 0:
                    return "Command name is missing.";
                case > 1:
                    return "Command name can only be a single word, no whitespace allowed.";
            }
        
            var name = inlineArgs.First();
            if (name.Any(char.IsWhiteSpace))
            {
                return "Command name can only be a single word, no whitespace allowed.";
            }
        
            Command = new CustomCommand
            {
                Command = name
            };
        
            return true;
        },
        true,
        "!-- CustomCommand myban"
    );

    public override Argument[] Arguments =>
    [
        new(
            "availableFor",
            "Specifies from which console the command can be executed from. Accepts: " +
            Enum.GetNames(typeof(ConsoleType)).Without(nameof(ConsoleType.None)).JoinStrings(" or "),
            AddConsoleType,
            false,
            "-- availableFor Player RemoteAdmin"
        ),
        new(
            "description",
            "The description of the command.",
            AddDescription,
            false,
            "-- description \"Used to ban a player\""
        ),
        new(
            "neededPermission",
            """
            The permission that the player has to have in order to be able to use this command.
            You can provide multiple permissions, and if the player has any of the listed permissions, they will be able to use the command.
            """,
            AddNeededPermission,
            false,
            "-- neededPermission ban.use ban.bypass"
        ),
        new(
            "noPermissionMessage",
            "Defines a message for when the sender does not have the needed permission (defined in 'neededPermission' argument).",
            AddNoPermissionMessage,
            false,
            "-- noPermissionMessage \"You do not have the required permissions to ban a player!\""
        ),
        new(
            "neededRank",
            """
            The required remote admin rank in order to have access to this command.
            You can provide multiple ranks, and if the player has any of the listed ranks, they will be able to use the command.
            """,
            AddNeededRank,
            false,
            "-- neededRank owner admin moderator staff"
        ),
        new(
            "invalidRankMessage",
            "Defines a message for when the sender does not have the needed rank (defined in 'neededRank' argument).",
            AddInvalidRankMessage,
            false,
            "-- invalidRankMessage \"You are not server staff!\""
        ),
        new(
            "cooldown",
            "The time the player has to wait before being able to use the command again.",
            args => AddCooldown(args, false),
            false,
            "-- cooldown 30s"
        ),
        new(
            "onCooldownMessage",
            """
            Defines a message for when the player tries to run a command but is on cooldown. 
            Additionally, you can use %time% in the message to show the remaining time (in seconds) the player has to wait before being able to use the command again.
            """,
            args => AddOnCooldownMessage(args, false),
            false,
            "-- onCooldownMessage \"You are on cooldown! You can use this command in %time% seconds.\""
        ),
        new(
            "globalCooldown",
            """
            The time all players have to wait before being able to use the command again.
            If anyone uses a command, everyone else will be unable to use it for the specified duration.
            This also applies to the server console.
            """,
            args => AddCooldown(args, true),
            false,
            "-- globalCooldown 30s"
        ),
        new(
            "onGlobalCooldownMessage",
            """
            Defines a message for when someone tries to run a command but is on global cooldown. 
            Additionally, you can use %time% in the message to show the remaining time (in seconds) someone has to wait before being able to use the command.
            """,
            args => AddOnCooldownMessage(args, true),
            false,
            "-- onGlobalCooldownMessage \"This command is on cooldown! You can use this command in %time% seconds\""
        ),
        new(
            "arguments",
            """
            The arguments that this command expects in order to run. 
            The script cannot run unless every single argument is specified. 
            
            When the command is ran, the provided values for the arguments turn into their own literal local variables for you to use in the script. 
            For example: making a command with an argument 'name' will then create a local variable '$name' in your script.
            
            If you want some arguments to be optional, you can use the '?' symbol after the argument name. 
            For example: 'reason?' argument will be optional, and if the sender does not provide a value for it, the script will still run.
            Then, the '$reason' command will not be created in your script.
            
            (the '?' suffix is NOT added to the variable names)
            """,
            AddArguments,
            false,
            "-- arguments id time reason?"
        )
    ];
    
    [Flags]
    public enum ConsoleType
    {
        None        = 0,
        Player      = 1 << 0,
        RemoteAdmin = 1 << 1,
        Server      = 1 << 2
    }

    public override void OnParsingComplete()
    {
        if (ScriptCommands.ContainsKey(Command))
        {
            return;
        }
        
        ScriptCommands.Add(Command, this);
        
        foreach (var console in Command.ConsoleTypes.GetFlags())
        {
            switch (console)
            {
                case ConsoleType.Player:
                    QueryProcessor.DotCommandHandler.RegisterCommand(Command);
                    continue;
                case ConsoleType.Server:
                    Console.ConsoleCommandHandler.RegisterCommand(Command);
                    continue;
                case ConsoleType.RemoteAdmin:
                    CommandProcessor.RemoteAdminCommandHandler.RegisterCommand(Command);
                    continue;
                case ConsoleType.None:
                default:
                    throw new AndrzejFuckedUpException();
            }
        }
    }

    public override void Unbind()
    {
        ScriptCommands.Remove(Command);
        
        foreach (var console in Command.ConsoleTypes.GetFlags())
        {
            switch (console)
            {
                case ConsoleType.Player:
                    QueryProcessor.DotCommandHandler.UnregisterCommand(Command);
                    break;
                case ConsoleType.Server:
                    Console.ConsoleCommandHandler.UnregisterCommand(Command);
                    break;
                case ConsoleType.RemoteAdmin:
                    CommandProcessor.RemoteAdminCommandHandler.UnregisterCommand(Command);
                    break;
                case ConsoleType.None:
                default:
                    throw new AndrzejFuckedUpException();
            }
        }
    }

    public override Result OnScriptRunning(Script scr)
    {
        if (scr.HasFlag<OnEventFlag>())
        {
            return $"Detected conflicting flag: {nameof(OnEventFlag)}.";
        }
        
        if (scr.RunReason == RunReason.CustomCommand)
        {
            return true;
        }

        return $"Tried to run script by other mean than the '{Command.Command}' command, which is not allowed.";
    }

    public class CustomCommand : ICommand, IUsageProvider, IHelpProvider
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (RunAttachedScript(this, ScriptExecutor.Get(sender), arguments.ToArray())
                .HasErrored(out var error))
            {
                response = error;
                return false;       
            }
        
            response = "Command executed.";
            return true;
        }

        public required string Command { get; init; }
        public string[] Aliases => [];
        public string Description { get; set; } = "";
        public string[] Usage { get; set; } = [];
        public ConsoleType ConsoleTypes = ConsoleType.Server;
        
        public string[] NeededRanks = [];
        public string? InvalidRankMessage;
        
        public string[] NeededPermissions = [];
        public string? NoPermissionMessage;
        
        public TimeSpan PlayerCooldown = TimeSpan.Zero;
        public Dictionary<Player, DateTime> NextEligibleDateForPlayer { get; } = [];
        public string? OnCooldownMessage;
        
        public TimeSpan GlobalCooldown = TimeSpan.Zero;
        public DateTime? NextEligibleDateForGlobal;
        public string? OnGlobalCooldownMessage;
        
        public string GetHelp(ArraySegment<string> arguments)
        {
            return $"Description: {Description}\n" +
                   $"Arguments: {Usage.Select(arg => $"[{arg}]").JoinStrings(" ")}";
        }
    }

    public static readonly Dictionary<CustomCommand, CustomCommandFlag> ScriptCommands = [];

    public CustomCommand Command = null!;

    public static Result RunAttachedScript(CustomCommand cmd, ScriptExecutor sender, string[] args)
    {
        if (sender is IPlayerExecutor { Player: { } player } && HandlePlayer(cmd, player) is { } plrErr)
        {
            return plrErr;
        }

        if (cmd.GlobalCooldown > TimeSpan.Zero)
        {
            if (cmd.NextEligibleDateForGlobal is { } nextEligibleDate
                && nextEligibleDate > DateTime.UtcNow)
            {
                var timeRemaining = Math.Round(
                    (nextEligibleDate - DateTime.UtcNow).TotalSeconds, 
                    MidpointRounding.AwayFromZero
                );
                
                if (cmd.OnGlobalCooldownMessage is not null)
                {
                    return cmd.OnGlobalCooldownMessage.Replace("%time%", timeRemaining.ToString("F1"));
                }

                return $"This command is on cooldown! You will be able to use this command in {timeRemaining} seconds.";
            }
            
            cmd.NextEligibleDateForGlobal = DateTime.UtcNow + cmd.GlobalCooldown;
        }
        
        if (!ScriptCommands.TryGetValue(cmd, out var flag))
        {
            return "The script that was supposed to handle this command was not found.";
        }

        if (Tokenizer.SliceLine(args.JoinStrings(" "))
            .HasErrored(out var sliceError, out var outSlices))
        {
            return sliceError;
        }

        var provided = outSlices.ToArray();
        var requiredArgumentLen = cmd.Usage.Count(arg => arg.Last() != '?');
        if (provided.Length < requiredArgumentLen)
        {
            return "Not enough arguments. " +
                   $"Expected at least {requiredArgumentLen}, but got {provided.Length}.";
        }

        if (provided.Length > cmd.Usage.Length)
        {
            return "Too many arguments. " +
                   $"Expected at most {cmd.Usage.Length}, but got {provided.Length}.";
        }

        if (Script.CreateByScriptName(flag.ScriptName, sender)
            .HasErrored(out var error, out var script))
        {
            return error;
        }

        for (var index = 0; index < provided.Length; index++)
        {
            var slice = provided[index];
            var argVariable = cmd.Usage[index];
            var name = argVariable[0].ToString().ToLower() + argVariable[1..];

            if (name.Last() == '?')
            {
                name = name[..^1];
            }

            if (Tokenizer.GetTokenFromSlice(slice, null!, 0)
                .WasSuccessful(out var token))
            {
                if (token.TryGetLiteralValue<LiteralValue>().WasSuccessful(out var value))
                {
                    script.AddLocalVariable(Variable.Create(name, value));
                    continue;
                }
            }
            
            script.AddLocalVariable(new LiteralVariable<TextValue>(name, new StaticTextValue(slice.Value)));
        }

        script.AddLocalVariable(new ReferenceVariable("command", new ReferenceValue(cmd)));
        script.Run(RunReason.CustomCommand);
        return true;
    }

    private static string? HandlePlayer(CustomCommand cmd, Player plr)
    {
        Log.Debug($"handling player in command {cmd.Command}");
        if (cmd.NeededRanks.Any())
        {
            if (plr.UserGroup is not { } group || cmd.NeededRanks.All(rank => group.Name != rank))
            {
                if (cmd.InvalidRankMessage is not null)
                {
                    return cmd.InvalidRankMessage;
                }
                
                return "This command is reserved for players with a rank: " +
                       $"{cmd.NeededRanks.Select(r => $"'{r}'").JoinStrings(" or ")}";
            }
        }

        if (cmd.NeededPermissions.Any() && !plr.HasAnyPermission(cmd.NeededPermissions))
        {
            if (cmd.NoPermissionMessage is not null)
            {
                return cmd.NoPermissionMessage;
            }
            
            return "You do not have one of the required permissions to use this command. " +
                   $"Required permissions: {cmd.NeededPermissions.JoinStrings(", ")}.";    
        }
        
        if (cmd.PlayerCooldown <= TimeSpan.Zero)
        {
            return null;
        }
        
        if (cmd.NextEligibleDateForPlayer.TryGetValue(plr, out var nextEligibleDate) && nextEligibleDate > DateTime.UtcNow)
        {
            var timeRemaining = Math.Round(
                (nextEligibleDate - DateTime.UtcNow).TotalSeconds,
                MidpointRounding.AwayFromZero
            );
            
            if (cmd.OnCooldownMessage is not null)
            {
                return cmd.OnCooldownMessage.Replace("%time%", timeRemaining.ToString("F1"));
            }
            
            return $"You are on cooldown! You will be able to use this command in {timeRemaining} seconds.";
        }
        
        cmd.NextEligibleDateForPlayer[plr] = DateTime.UtcNow + cmd.PlayerCooldown;
        return null;
    }
    
    private Result AddArguments(string[] args)
    {
        var onlyOptionals = false;
        foreach (var arg in args)
        {
            var markedOptional = arg.Last() == '?';
            if (markedOptional)
            {
                onlyOptionals = true;
            }
            else if (onlyOptionals && !markedOptional)
            {
                return $"Argument '{arg}' is not optional, but previous arguments were marked as optional.";
            }

            var varName = markedOptional ? arg[..^1] : arg;
            if (!varName.All(char.IsLetter))
            {
                return $"Argument '{arg}' contains non-letter characters.";
            }
            
            if (markedOptional && arg.Length == 1)
            {
                return $"You cannot provide a {arg} as an argument.";
            }
        }

        Command.Usage = args;
        return true;
    }

    private Result AddConsoleType(string[] args)
    {
        ConsoleType types = default;
        foreach (var arg in args)
        {
            if (Enum.TryParse(arg, true, out ConsoleType consoleType))
            {
                types |= consoleType;
                continue;
            }

            return $"Value '{arg}' is not a valid {nameof(ConsoleType)}";
        }
        
        Command.ConsoleTypes = types;
        return true;
    }

    private Result AddDescription(string[] args)
    {
        Command.Description = args.JoinStrings(" ");
        return true;
    }

    private Result AddNeededPermission(string[] args)
    {
        Command.NeededPermissions = args;
        return true;
    }
    
    private Result AddNoPermissionMessage(string[] args)
    {
        Command.NoPermissionMessage = args.JoinStrings(" ");
        return true;
    }
    
    private Result AddNeededRank(string[] args)
    {
        Command.NeededRanks = args;
        return true;
    }

    private Result AddInvalidRankMessage(string[] args)
    {
        Command.InvalidRankMessage = args.JoinStrings(" ");
        return true;
    }
    
    private Result AddCooldown(string[] args, bool isGlobal)
    {
        switch (args.Length)
        {
            case < 1: return "Cooldown requires a duration value.";
            case > 2: return $"Cooldown expects only a single duration value, got given {args.Length} instead.";
        }

        var rawValue = args[0];
        if (Tokenizer.GetTokenFromString(rawValue, null, null).HasErrored(out _, out var token) 
            || token is not DurationToken durationToken)
        {
            return $"Value '{rawValue}' is not a valid duration.";
        }

        if (isGlobal)
        {
            Command.GlobalCooldown = durationToken.Value;
        }
        else
        {
            Command.PlayerCooldown = durationToken.Value;
        }
        
        return true;
    }

    private Result AddOnCooldownMessage(string[] args, bool isGlobal)
    {
        if (isGlobal)
        {
            Command.OnGlobalCooldownMessage = args.JoinStrings(" ");
        }
        else
        {
            Command.OnCooldownMessage = args.JoinStrings(" ");
        }
        
        return true;
    }
}