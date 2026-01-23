using System.Reflection;
using System.Text;
using CommandSystem;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.FlagSystem.Flags;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.Extensions;
using SER.Code.MethodSystem;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.MethodSystem.BaseMethods.Interfaces;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.BaseMethods.Yielding;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.Plugin.Commands.Interfaces;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.ExpressionTokens;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem;
using SER.Code.VariableSystem.Variables;
using EventHandler = SER.Code.EventSystem.EventHandler;

namespace SER.Code.Plugin.Commands.HelpSystem;

[CommandHandler(typeof(GameConsoleCommandHandler))]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class HelpCommand : ICommand
{
    public string Command => MainPlugin.HelpCommandName;
    public string[] Aliases => [];
    public string Description => "The help command of SER.";

    public static readonly Dictionary<HelpOption, Func<string>> GeneralOptions = new()
    {
        [HelpOption.Methods] = GetMethodList,
        [HelpOption.Variables] = GetVariableList,
        [HelpOption.Enums] = GetEnumHelpPage,
        [HelpOption.Events] = GetEventsHelpPage,
        [HelpOption.RefResMethods] = GetReferenceResolvingMethodsHelpPage,
        [HelpOption.PlayerProperty] = GetPlayerInfoAccessorsHelpPage,
        [HelpOption.Flags] = GetFlagHelpPage,
        //[HelpOption.Keywords] = GetKeywordHelpPage
    };
    
    public bool Execute(ArraySegment<string> arguments, ICommandSender _, out string response)
    {
        if (arguments.Count > 0)
        {
            return GetGeneralOutput(arguments.First().ToLower(), out response);
        }

        response = GetOptionsList();
        return true;
    }

    public static bool GetGeneralOutput(string arg, out string response)
    {
        if (Enum.TryParse(arg, true, out HelpOption option))
        {
            if (!GeneralOptions.TryGetValue(option, out var func))
            {
                throw new AndrzejFuckedUpException($"Option {option} was not added to the help system.");
            }
            
            response = func();
            return true;
        }
        
        var keyword = KeywordToken.KeywordContextTypes
            .Select(kType => kType.CreateInstance<IKeywordContext>())
            .FirstOrDefault(keyword => keyword.KeywordName == arg);
        
        if (keyword is not null)
        {
            response = GetKeywordInfo(
                keyword.KeywordName,
                keyword.Description,
                keyword.Arguments,
                keyword is StatementContext,
                keyword.GetType()
            );
            return true;
        }
        
        var enumType = HelpInfoStorage.UsedEnums.FirstOrDefault(e => e.Name.ToLower() == arg);
        if (enumType is not null)
        {
            response = GetEnum(enumType);
            return true;
        }
        
        var ev = EventHandler.AvailableEvents
            .FirstOrDefault(e => e.Name.ToLower() == arg);
        if (ev is not null)
        {
            response = GetEventInfo(ev);
            return true;
        }
        
        var method = MethodIndex.GetMethods()
            .FirstOrDefault(met => met.Name.ToLower() == arg);
        if (method is not null)
        {
            response = GetMethodHelp(method);
            return true;
        }

        var correctFlagName = Flag.FlagInfos.Keys
            .FirstOrDefault(k => k.ToLower() == arg);
        if (correctFlagName is not null)
        {
            response = GetFlagInfo(correctFlagName);
            return true;
        }

        response = $"There is no '{arg}' option!";
        return false;
    }

    private static string GetOptionsList()
    {
        return $"""
                === Welcome to the help command of SER! ===

                To get specific information for your script creation adventure:
                (1) find the desired option (like '{nameof(HelpOption.Methods).ToLower()}')
                (2) use this command, attaching the option after it (like 'serhelp methods')
                (3) enjoy!

                Here are all the available options:
                > {"\n> ".Join(Enum.GetValues(typeof(HelpOption)).Cast<HelpOption>()
                    .Select(o => o.ToString().LowerFirst()))}
                    
                    
                === Other commands! ===
                > {"\n> ".Join(Assembly.GetExecutingAssembly().GetTypes()
                    .Where(t => typeof(ICommand).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract && t != typeof(HelpCommand))
                    .Select(Activator.CreateInstance)
                    .Cast<ICommand>()
                    .Where(c => !string.IsNullOrEmpty(c.Command))
                    .Select(c 
                        => $"{c.Command} (permission: {(c as IUsePermissions)?.Permission ?? "not required"})" + 
                           $"\n{(string.IsNullOrEmpty(c.Description) ? string.Empty : c.Description + "\n")}"))}
                """;
    }

    private static string GetKeywordInfo(string name, string description, string[] arguments, bool isStatement, Type type)
    {
        var usageInfo = Activator.CreateInstance(type) is IStatementExtender extender
            ? $"""
               --- Usage ---
               This statement can ONLY be used after a statement supporting the "{extender.Extends}" signal!

               # example usage (assuming "somekeyword" supports "{extender.Extends}" signal)
               
               somekeyword
                   # some code
               {name} {arguments.JoinStrings(" ")}
                   # some other code
               end
               """
            : $"""
               --- Usage ---
               {name} {arguments.JoinStrings(" ")}
               {(isStatement ? "\t# some code\nend" : string.Empty)}
               """;
        
        var extendableInfo = Activator.CreateInstance(type) is IExtendableStatement extendable
            ? $"""
               --- This statement is extendable! ---
               Other statements can be added after this one, provided they support one of the following signal(s):
               {extendable.AllowedSignals.GetFlags().Select(f => $"> {f}").JoinStrings("\n")}
               """
            : string.Empty;
        
        return 
            $"""
            ===== {name} keyword =====
            > {description}
            
            {usageInfo}
            
            {extendableInfo}
            """;
    }

    /*private static string GetKeywordHelpPage()
    {
        return
            """
            Keywords are "commands" that alter how the script should behave.
            They can range from simple things like stopping the script, to more complex things like handling advanced logic.

            Keywords are written as all lowercase words, like 'stop', 'if' etc.

            Some keywords also have an ability to house methods inside their "body", making them _statements_.
            These statements control how the methods inside their body are executed.

            For example:
            if 5 > 3
                # here is some code
                # which will only run if the "if" statement is true
            end

            Or another example:
            repeat 10
                # here is some code
                # which will run 10 times
            end

            Here is a list of all keywords available in SER:
            (each of them is of course searchable using 'serhelp keywordName')
            """ + KeywordToken.KeywordTypes
                .Select(t => t.CreateInstance<IKeywordContext>())
                .Select(k => $"{k.KeywordName} - {k.Description}")
                .JoinStrings("\n");

        var keywords = KeywordToken.KeywordInfo.Keys.Select(k => $"\n> {k}").JoinStrings();

        return
            """
            Keywords are special "commands" that alter how the script is executed.
            They can range from simple things like stopping the script, to more complex things like handling advanced logic.

            Keywords are written as all lowercase words, like 'stop' or 'if'.

            Some keywords also have an ability to house methods inside their "body", making them statements.
            These statements control how the methods inside their body are executed.

            For example:
            if ...
                # here is some code
                # which will only run if the "if" statement is true
            end

            Or another example:
            repeat 10
                # here is some code
                # which will run 10 times
            end

            Here is a list of all keywords available in SER:
            (each of them is of course searchable using 'serhelp keywordName')
            """ + keywords;
    }*/

    private static string GetFlagHelpPage()
    {
        var flags = Flag.FlagInfos.Keys
            .Select(f => $"> {f}")
            .JoinStrings("\n");
        
        return
            $"""
            Flags are a way to change script behavior depending on your needs.
            
            This how they are used:
            !-- SomeFlag argValue1 argValue2
            -- customFlagArgument "some value"
            
            Flags should be used at the top of the script.
            
            Below is a list of all flags available in SER:
            (for more info about their usage, use 'serhelp flagName')
            {flags}
            """;
    }

    private static string GetFlagInfo(string flagName)
    {
        var flag = Flag.FlagInfos[flagName].CreateInstance<Flag>();
        
        var inlineArgumentUsage = flag.InlineArgument.HasValue
            ? "..."
            : string.Empty;
        
        var argumentsUsage = flag.Arguments
            .Select(arg => $"-- {arg.Name} ...")
            .JoinStrings("\n");

        StringBuilder argumentDescription = new();
        if (flag.InlineArgument.HasValue)
        {
            argumentDescription.AppendLine($"  Inline argument '{flag.InlineArgument.Value.Name}':");
            argumentDescription.AppendLine($"  > {flag.InlineArgument.Value.Description}");
            argumentDescription.AppendLine();
        }

        foreach (var arg in flag.Arguments)
        {
            argumentDescription.AppendLine($"  Additional argument '{arg.Name}':");
            argumentDescription.AppendLine($"  > {arg.Description}");
            
            if (!arg.IsRequired)
            {
                argumentDescription.AppendLine($"  > This argument is not required for the flag to operate");
            }
            
            argumentDescription.AppendLine();
        }
        
        return
            $"""
             ===== {flagName} =====
             > {flag.Description}
             
             Usage:
             !-- {flagName} {inlineArgumentUsage}
             {argumentsUsage}
             
             {(argumentDescription.Length > 0 ? "Arguments:" : "")}
             {argumentDescription}
             """;
    }

    private static string GetEventInfo(EventInfo ev)
    {
        var variables = EventHandler.GetMimicVariables(ev);
        var msg = variables.Count > 0 
            ? variables.Aggregate(
                "This event has the following variables attached to it:\n", 
                (current, variable) => current + $"> {variable}\n"
            ) 
            : "This event does not have any variables attached to it.";
        
        return 
             $"""
              Event {ev.Name} is a part of {ev.DeclaringType?.Name ?? "unknown event group"}.
              
              {msg}
              """;
    }
    
    private static string GetReferenceResolvingMethodsHelpPage()
    {
        var referenceResolvingMethods = MethodIndex.GetMethods()
            .Where(m => m is IReferenceResolvingMethod)
            .Select(m => (m.Name, ((IReferenceResolvingMethod)m).ReferenceType));
        
        var sb = new StringBuilder();
        foreach (var method in referenceResolvingMethods)
        {
            sb.AppendLine($"{method.ReferenceType.GetAccurateName()} ref -> {method.Name} method");
        }
        
        return
            $"""
             Reference resolving methods are methods that help you extract information from a given reference.
             This help option is just here to make it easier to find said methods.
             
             Here are all reference resolving methods:
             {sb}
             """;
    }

    private static string GetEventsHelpPage()
    {
        var sb = new StringBuilder();
        
        foreach (var category in EventHandler.AvailableEvents.Select(ev => ev.DeclaringType).ToHashSet().OfType<Type>())
        {
            sb.AppendLine($"--- {category.Name} ---");
            sb.AppendLine(string.Join(", ",  EventHandler.AvailableEvents
                .Where(ev => ev.DeclaringType == category)
                .Select(ev => ev.Name)));
        }
        
        return
            $"""
            Event is a signal that something happened on the server. 
            If the round has started, server will invoke an event (signal) called RoundStarted.
            You can use this functionality to run your scripts when a certain event happens.
            
            By putting `!-- OnEvent RoundStarted` at the top of your script, you will run your script when the round starts.
            You can put something different there, e.g. `!-- OnEvent Death`, which will run when someone has died.
            
            Some events have additional information attached to them in a form of variables.
            If you wish to know what variables are available for a given event, just use 'serhelp <eventName>'!
            
            Here are all events that SER can use:
            {sb}
            """;
    }
    
    private static string GetEnum(Type enumType)
    {
        return
            $"""
            Enum {enumType.Name} has the following values:
            {string.Join("\n", Enum.GetValues(enumType).Cast<Enum>().Select(e => $"> {e}"))}
            """;
    }

    private static string GetEnumHelpPage()
    {
        return 
            $"""
            Enums are basically options, where an enum has set of all valid values, so a valid option is an enum value.
            These enums are usually used to specify a room, door, zone etc.
            
            To get the list of all available values that an enum has, just use 'serhelp <enumName>'.
            For example: 'serhelp RoomName' will get you a list of all available room names to use in methods.
            
            Here are all enums used in SER:
            {string.Join("\n", HelpInfoStorage.UsedEnums.Select(e => $"> {e.Name}"))}
            """;
    }

    private static string GetMethodList()
    {
        var methods = MethodIndex.GetMethods();
        const string retsPrefix = " [rets]";
        
        Dictionary<string, List<Method>> methodsByCategory = new();
        foreach (var method in methods)
        {
            if (methodsByCategory.ContainsKey(method.Subgroup))
            {
                methodsByCategory[method.Subgroup].Add(method);
            }
            else
            {
                methodsByCategory.Add(method.Subgroup, [method]);
            }
        }
        
        var sb = new StringBuilder($"Hi! There are {methods.Length} methods available for your use!\n");
        sb.AppendLine("If a method has [rets], it means that this method returns a value.");
        sb.AppendLine("If you want to get specific information about a given method, just do 'serhelp <MethodName>'!");
        
        foreach (var kvp in methodsByCategory.Reverse())
        {
            var descDistance = kvp.Value
                .Select(m => m.Name.Length + (m is ReturningMethod ? retsPrefix.Length : 0))
                .Max() + 1;
            
            sb.AppendLine();
            sb.AppendLine($"--- {kvp.Key} methods ---");
            foreach (var method in kvp.Value)
            {
                var name = method.Name;
                if (method is ReturningMethod)
                {
                    name += " [rets]";
                }

                var descDistanceString = new string(' ', descDistance - name.Length);
                var desc = method.Description 
                           ?? $"Extracts information from {((IReferenceResolvingMethod)method).ReferenceType.Name} objects.";
                
                sb.AppendLine($"> {name}{descDistanceString}~ {desc}");
            }
        }
        
        return sb.ToString();
    }
    
    private static string GetVariableList()
    {
        var allVars = VariableIndex.GlobalVariables
            .Where(var => var is PredefinedPlayerVariable)
            .Cast<PredefinedPlayerVariable>()
            .ToList();
        
        var sb = new StringBuilder($"Hi! There are {allVars.Count} variables available for your use!\n");
        
        var categories = allVars.Select(var => var.Category).Distinct().ToList();
        foreach (var category in categories)
        {
            sb.AppendLine();
            sb.AppendLine($"--- {category ?? "Other"} variables ---");
            foreach (var var in allVars.Where(var => var.Category == category))
            {
                sb.AppendLine($"> @{var.Name}");
            }
        }
        
        return sb.ToString();
    }

    private static string GetMethodHelp(Method method)
    {
        var sb = new StringBuilder($"=== {method.Name} ===\n");

        sb.AppendLine($"> {method.Description}");
        
        if (method is IAdditionalDescription addDesc)
        {
            sb.AppendLine();
            sb.AppendLine($"> {addDesc.AdditionalDescription}");
        }
        
        switch (method)
        {
            case LiteralValueReturningMethod ret:
            {
                string typeReturn;
                if (ret.LiteralReturnTypes.AreKnown(out var types))
                {
                    typeReturn = types
                        .Select(Value.FriendlyName)
                        .JoinStrings(" or ") + " value";
                }
                else
                {
                    typeReturn = "literal value depending on your input";
                }

                sb.AppendLine();
                sb.AppendLine($"This method returns a {typeReturn}, which can be saved or used directly. ");
                break;
            }
            case IReturningMethod<CollectionValue>:
                sb.AppendLine();
                sb.AppendLine("This method returns a collection of values, which can be saved or used directly.");
                break;
            case IReturningMethod<PlayerValue>:
                sb.AppendLine();
                sb.AppendLine("This method returns players, which can be saved or used directly.");
                break;
            case IReferenceReturningMethod refMethod:
                sb.AppendLine();
                sb.AppendLine($"This method returns a reference to {refMethod.ReturnType.GetAccurateName()} object, which can be saved or used directly.\n" +
                              $"References represent an object which cannot be fully represented in text.\n" +
                              $"If you wish to use that reference further, find methods supporting references of this type.");
                break;
            case IReturningMethod ret:
            {
                string typeReturn;
                if (ret.Returns.AreKnown(out var returnTypes))
                {
                    typeReturn = returnTypes
                        .Select(Value.FriendlyName)
                        .JoinStrings(" or ") + " value";
                }
                else
                {
                    typeReturn = "value depending on your input";
                }
                
                sb.AppendLine();
                sb.AppendLine($"This method returns a {typeReturn}, which can be saved or used directly. ");
                break;
            }
        } 

        if (method.ExpectedArguments.Length == 0)
        {
            sb.AppendLine();
            sb.AppendLine("This method does not expect any arguments.");
            return sb.ToString();
        }
        
        sb.AppendLine();
        sb.AppendLine("This method expects the following arguments:");
        for (var index = 0; index < method.ExpectedArguments.Length; index++)
        {
            if (index > 0) sb.AppendLine();
            
            var argument = method.ExpectedArguments[index];
            var optionalArgPrefix = argument.DefaultValue is not null ? " optional" : "";
            sb.AppendLine($"({index + 1}){optionalArgPrefix} '{argument.Name}' argument");

            if (argument.Description is not null)
            {
                sb.AppendLine($" - Description: {argument.Description}");
            }
            
            sb.AppendLine($" - Expected value: {argument.InputDescription.Replace("\n", "\n\t")}");

            if (argument.DefaultValue is { } defVal)
            {
                sb.AppendLine($" - Default value: {defVal.StringRep ?? defVal.Value?.ToString() ?? "unknown"}");
            }

            if (argument.ConsumesRemainingValues)
            {
                sb.AppendLine(
                    " - This argument consumes all remaining values; this means that every value provided AFTER " +
                    "this one will ALSO count towards this argument's values.");
            }
        }

        if (method is ICanError errorMethod)
        {
            sb.AppendLine();
            sb.AppendLine("This method defines custom errors:");
            sb.AppendLine(errorMethod.ErrorReasons.Select(e => $"> {e}").JoinStrings("\n"));
        }

        /*if (method.ExpectedArguments.All(arg => arg.AdditionalDescription is null))
        {
            return sb.ToString();
        }

        sb.AppendLine("\nAdditional information about arguments:");
        for (var index = 0; index < method.ExpectedArguments.Length; index++)
        {
            var argument = method.ExpectedArguments[index];
            if (argument.AdditionalDescription is null) continue;
            
            sb.AppendLine($"({index + 1}) '{argument.Name}' argument");
            sb.AppendLine($" - {argument.AdditionalDescription}");
            sb.AppendLine();
        }*/
        
        return sb.ToString();
    }

    public static string GetPlayerInfoAccessorsHelpPage()
    {
        StringBuilder sb = new();
        var properties = PlayerExpressionToken.PropertyInfoMap;
        foreach (var (property, info) in properties.Select(kvp => (kvp.Key, kvp.Value)))
        {
            sb.Append($"{property.ToString().LowerFirst()} -> {info.ReturnType}");
            sb.Append(info.Description is not null ? $" | {info.Description}\n" : "\n");
        }

        return
            $$"""
            In order for you to get information about a player, you need to use a special syntax involving expressions.
            
            This syntax works as follows: {@plr property}
            > @plr: is a player variable with exactly 1 player stored in it
            > property: is a property of the player we want to get information about (its a {{nameof(PlayerExpressionToken.PlayerProperty)}} enum)
            
            Here is a list of all available properties and what they return:
            {{sb}}
            """;

        /*var accessors = PlayerPropertyAccessToken.AccessiblePlayerProperties.Select(kvp =>
        {
            if (kvp.Key.Item1 is { } name)
            {
                return $".{name}\nReturns: {kvp.Value.description}";
            }

            if (kvp.Key.Item2 is { } names)
            {
                return $"{names.Select(n => $".{n}").JoinStrings(" or ")}\nReturns: {kvp.Value.description}";
            }

            throw new AndrzejFuckedUpException();
        }).JoinStrings("\n\n");

        return
            """
            Player property accessors are suffixes added to a player variable with a single player to extract information about said player.

            Assuming you have a variable called '@myPlayer' that has 1 player, you can access properties like:
            name:   @myPlayer.name
            role:   @myPlayer.role
            health: @myPlayer.health
            etc.

            This works like any other literal variable, so you can save it to a variable, use it in a method, etc.

            Here is a list of all player property accessors and their definitions:

            """ + accessors;*/
    }
}











