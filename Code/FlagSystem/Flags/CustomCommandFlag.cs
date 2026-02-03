using CommandSystem;
using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using RemoteAdmin;
using SER.Code.Helpers;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.ScriptSystem.Structures;
using SER.Code.TokenSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Bases;
using SER.Code.VariableSystem.Variables;
using Console = GameCore.Console;

namespace SER.Code.FlagSystem.Flags;

[UsedImplicitly]
public class CustomCommandFlag : Flag
{
    public override string Description => 
        "Creates a command and binds it to the script. When the command is ran, it executes the script.";
    
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
        true
    );

    public override Argument[] Arguments =>
    [
        new(
            "arguments",
            "The arguments that this command expects in order to run. " +
            "The script cannot run unless every single argument is specified. " +
            "When the command is ran, the provided values for the arguments turn into their own literal local " +
            "variables for you to use in the script. " +
            "For example: making a command with an argument 'name' will then create a local variable $name in your script. " +
            "Side note: when a player is running the command, a @sender local player variable will also be created.",
            AddArguments,
            false
        ),
        new(
            "availableFor",
            $"Specifies from which console the command can be executed from. Accepts {nameof(ConsoleType)} enum values.",
            AddConsoleType,
            false
        ),
        new(
            "description",
            "The description of the command.",
            AddDescription,
            false
        ),
        new(
            "neededRank",
            "The required remote admin rank in order to have access to this command. " +
            "You can provide multiple ranks, and if the player has any of the listed ranks, they will be able to use the command.",
            AddNeededRank,
            false
        ),
        new(
            "cooldown",
            "The time the player has to wait before being able to use the command again.",
            AddCooldown,
            false
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
                    continue;
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
                    continue;
                default:
                    throw new AndrzejFuckedUpException();
            }
        }
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
        public string[] Aliases { get; set; } = [];
        public string Description { get; set; } = "";
        public ConsoleType ConsoleTypes { get; set; } = ConsoleType.Server;
        public string[] Usage { get; set; } = [];
        public string[] NeededRanks { get; set; } = [];
        public TimeSpan PlayerCooldown { get; set; } = TimeSpan.Zero;
        public Dictionary<Player, DateTime> NextEligableDateForPlayer { get; } = [];
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
        
        if (!ScriptCommands.TryGetValue(cmd, out var flag))
        {
            return "The script that was supposed to handle this command was not found.";
        }

        if (Tokenizer.SliceLine(args.JoinStrings(" "))
            .HasErrored(out var sliceError, out var outSlices))
        {
            return sliceError;
        }

        var slices = outSlices.ToArray();
        if (slices.Length < cmd.Usage.Length)
        {
            return "Not enough arguments. " +
                   $"Expected {cmd.Usage.Length} but got {slices.Length}.";
        }

        if (slices.Length > cmd.Usage.Length)
        {
            return "Too many arguments. " +
                   $"Expected {cmd.Usage.Length} but got {slices.Length}.";
        }

        if (Script.CreateByScriptName(flag.ScriptName, sender)
            .HasErrored(out var error, out var script))
        {
            return error;
        }

        for (var index = 0; index < cmd.Usage.Length; index++)
        {
            var slice = slices[index];
            var argVariable = cmd.Usage[index];
            var name = argVariable[0].ToString().ToLower() + argVariable[1..];

            if (Tokenizer.GetTokenFromSlice(slice, null!, 0)
                .WasSuccessful(out var token))
            {
                if (token.TryGetLiteralValue<LiteralValue>().WasSuccessful(out var value))
                {
                    script.AddVariable(Variable.Create(name, value));
                    continue;
                }
            }
            
            script.AddVariable(new LiteralVariable<TextValue>(name, new StaticTextValue(slice.Value)));
        }

        script.Run(RunContext.Command);
        return true;
    }

    private static string? HandlePlayer(CustomCommand cmd, Player plr)
    {
        Log.Debug($"handling player in command {cmd.Command}");
        if (cmd.NeededRanks.Any())
        {
            if (plr.UserGroup is not { } group || cmd.NeededRanks.All(rank => group.Name != rank))
            {
                return "This command is reserved for players with a rank: " +
                       $"{cmd.NeededRanks.Select(r => $"'{r}'").JoinStrings(" or ")}";
            }
        }

        if (cmd.PlayerCooldown <= TimeSpan.Zero)
        {
            return null;
        }
        
        if (cmd.NextEligableDateForPlayer.TryGetValue(plr, out var nextEligableDate) && nextEligableDate > DateTime.UtcNow)
        {
            return $"You are on cooldown! You can use this command in " +
                   $"{Math.Round((nextEligableDate - DateTime.UtcNow).TotalSeconds, MidpointRounding.AwayFromZero)} seconds.";
        }
        
        cmd.NextEligableDateForPlayer[plr] = DateTime.UtcNow + cmd.PlayerCooldown;
        return null;
    }
    
    private Result AddArguments(string[] args)
    {
        foreach (var arg in args)
        {
            if (!arg.All(char.IsLetter))
            {
                return $"Argument '{arg}' contains non-letter characters.";
            }
        }

        Command.Usage = args;
        return true;
    }

    private Result AddConsoleType(string[] args)
    {
        ConsoleType types = ConsoleType.None;

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
    
    private Result AddNeededRank(string[] args)
    {
        Command.NeededRanks = args;
        return true;
    }
    
    private Result AddCooldown(string[] args)
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

        Command.PlayerCooldown = durationToken.Value;
        return true;
    }
}