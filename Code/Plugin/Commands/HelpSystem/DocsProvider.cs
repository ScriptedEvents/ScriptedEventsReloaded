using System.Reflection;
using System.Text;
using CommandSystem;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.FlagSystem.Flags;
using SER.Code.Helpers;
using SER.Code.MethodSystem;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.MethodSystem.BaseMethods.Interfaces;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.Plugin.Commands.Interfaces;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.ExpressionTokens;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.Plugin.Commands.HelpSystem;

public static class DocsProvider
{
    public static readonly Dictionary<HelpOption, Func<string>> GeneralOptions = new()
    {
        [HelpOption.Methods] = GetMethodList,
        [HelpOption.Variables] = GetVariableList,
        [HelpOption.Enums] = GetEnumHelpPage,
        [HelpOption.Events] = GetEventsHelpPage,
        [HelpOption.RefResMethods] = GetReferenceResolvingMethodsHelpPage,
        [HelpOption.PlayerProperty] = GetPlayerInfoAccessorsHelpPage,
        [HelpOption.Flags] = GetFlagHelpPage,
        [HelpOption.Keywords] = GetKeywordHelpPage
    };

    public static bool GetGeneralOutput(string arg, out string response)
    {
        arg = arg.ToLower();
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
        
        var ev = EventSystem.EventHandler.AvailableEvents
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
        
        var outsideMethodKvp = MethodIndex.FrameworkDependentMethods
            .Select(kvp => kvp.Value.Select(m => (m, kvp.Key)))
            .Flatten()
            .FirstOrDefault(kvp => kvp.m.Name.ToLower() == arg);
        if (outsideMethodKvp is { m: {} outsideMethod, Key: var framework})
        {
            response = GetMethodHelp(outsideMethod, framework);
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

    public static string GetOptionsList()
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

    public static string GetKeywordInfo(string name, string description, string[] arguments, bool isStatement, Type type)
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
        
        // exampel
        var exampel = Activator.CreateInstance(type) is IKeywordContext { Example: {} e}
            ? $"""
               --- Example Usage ---
               {e}
               
               """
            : string.Empty;
        
        return 
            $"""
            ===== {name} keyword =====
            > {description}
            
            {usageInfo}
            {extendableInfo}
            {exampel}
            """;
    }

    public static string GetKeywordHelpPage()
    {
        return
            """
            Keywords alter how the script behaves, not by changing someones role, but the internal script execution.
            They can range from simple things from stopping the script to handling advanced logic.

            Keywords are written as all lowercase words, like 'stop', 'if' etc.

            Some keywords also have an ability to have instructions inside their "body", making them statements!
            These statements control how the methods inside their body are executed.

            Here is a list of all keywords available in SER:
            (each of them is of course searchable using 'serhelp keywordName')
            
            """ + KeywordToken.KeywordContextTypes
                .Select(t => t.CreateInstance<IKeywordContext>())
                .Select(k => $"> {k.KeywordName}")
                .JoinStrings("\n");
    }

    public static string GetFlagHelpPage()
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

    public static string GetFlagInfo(string flagName)
    {
        var flag = Flag.FlagInfos[flagName].CreateInstance<Flag>();
        
        var inlineArgumentUsage = flag.InlineArgument.HasValue
            ? "..."
            : string.Empty;
        
        var argumentsUsage = flag.Arguments
            .Select(arg => $"-- {arg.Name} ...")
            .JoinStrings("\n");

        StringBuilder argDesc = new();
        if (flag.InlineArgument.HasValue)
        {
            argDesc.AppendLine(
                (flag.InlineArgument.Value.IsRequired ? "> Required" : "> Optional") 
                + $" inline argument '{flag.InlineArgument.Value.Name}':"
            );
            argDesc.AppendLine($"{flag.InlineArgument.Value.Description}");
            argDesc.AppendLine("> Example usage");
            argDesc.AppendLine(flag.InlineArgument.Value.Example);
            argDesc.AppendLine();
        }

        foreach (var arg in flag.Arguments)
        {
            argDesc.AppendLine((arg.IsRequired ? "> Required" : "> Optional") + $" argument '{arg.Name}':");
            argDesc.AppendLine($"{arg.Description}");
            argDesc.AppendLine("> Example usage");
            argDesc.AppendLine(arg.Example);
            argDesc.AppendLine();
        }
        
        return
            $"""
             ===== {flagName} =====
             {flag.Description}
             
             Usage:
             !-- {flagName} {inlineArgumentUsage}
             {argumentsUsage}
             
             {(argDesc.Length > 0 ? "+++ Arguments +++" : "")}
             {argDesc}
             """;
    }

    public static string GetEventInfo(EventInfo ev)
    {
        var variables = EventSystem.EventHandler.GetMimicVariables(ev);
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
    
    public static string GetReferenceResolvingMethodsHelpPage()
    {
        var referenceResolvingMethods = MethodIndex.GetMethods()
            .Where(m => m is IReferenceResolvingMethod)
            .Select(m => (m.Name, ReferenceType: ((IReferenceResolvingMethod)m).ResolvesReference));
        
        var sb = new StringBuilder();
        foreach (var method in referenceResolvingMethods)
        {
            sb.AppendLine($"{method.ReferenceType.AccurateName} ref -> {method.Name} method");
        }
        
        return
            $"""
             Reference resolving methods are methods that help you extract information from a given reference.
             This help option is just here to make it easier to find said methods.
             
             Here are all reference resolving methods:
             {sb}
             """;
    }

    public static string GetEventsHelpPage()
    {
        var sb = new StringBuilder();
        
        foreach (var category in EventSystem.EventHandler.AvailableEvents.Select(ev => ev.DeclaringType).ToHashSet().OfType<Type>())
        {
            sb.AppendLine($"--- {category.Name} ---");
            sb.AppendLine(string.Join(", ",  EventSystem.EventHandler.AvailableEvents
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
    
    public static string GetEnum(Type enumType)
    {
        return
            $"""
            Enum {enumType.Name} has the following values:
            {string.Join("\n", Enum.GetValues(enumType)
                .Cast<Enum>()
                .Where(e => {
                    Type type = e.GetType();
                    FieldInfo field = type.GetField(e.ToString());
                    return field.GetCustomAttribute<ObsoleteAttribute>() == null;
                })
                .Select(e => $"> {e}"))}
            """;
    }

    public static string GetEnumHelpPage()
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

    private static Dictionary<string, List<Method>> MethodsByCategory()
    {
        Dictionary<string, List<Method>> methodsByCategory = new();
        foreach (var method in MethodIndex.GetMethods())
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
        
        return methodsByCategory;
    }

    public static string GetMethodList()
    {
        const string retsSuffix = " [rets]";

        var sb = new StringBuilder($"Hi! There are {MethodIndex.GetMethods().Length} methods available for your use!\n");
        sb.AppendLine($"If a method has {retsSuffix.TrimStart()}, it means that this method returns a value.");
        sb.AppendLine("If you want to get specific information about a given method, just do 'serhelp <MethodName>'!");

        foreach (var kvp in MethodsByCategory().OrderBy(kvp => kvp.Key[0]))
        {
            var descDistance = DescDistance(kvp.Value);
            
            sb.AppendLine();
            sb.AppendLine($"--- {kvp.Key} methods ---");
            foreach (var method in kvp.Value)
            {
                sb.AppendLine(GetFormatted(method, descDistance));
            }
        }
        
        foreach (var (framework, methods) in MethodIndex.FrameworkDependentMethods
                     .Where(kvp => FrameworkBridge.Found.All(fb => fb.Type != kvp.Key)))
        {
            var descDistance = DescDistance(methods);
            
            sb.AppendLine();
            sb.AppendLine($"--- (not accessible) {framework} framework methods ---");
            foreach (var method in methods)
            {
                sb.AppendLine(GetFormatted(method, descDistance));
            }
        }
        
        return sb.ToString();

        string GetFormatted(Method method, int descDistance)
        {
            var name = method.Name;
            if (method is ReturningMethod)
            {
                name += retsSuffix;
            }

            var descPadding = new string(' ', descDistance - name.Length);
            return $"> {name}{descPadding}~ {method.Description}";
        }
        
        int DescDistance(IEnumerable<Method> methods)
        {
            return methods
                .Select(m => m.Name.Length + (m is ReturningMethod ? retsSuffix.Length : 0))
                .Max() + 1;
        }
    }
    
    public static string GetVariableList()
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

    public static string GetMethodHelp(Method method, FrameworkBridge.Type? notLoadedFramework = null)
    {
        var sb = new StringBuilder($"=== {method.Name} ===\n");

        sb.AppendLine($"> {method.Description}");
        
        if (notLoadedFramework is {} framework)
        {
            sb.AppendLine();
            sb.AppendLine($"This method requires the '{framework}' framework in order to be used.");
            return sb.ToString();
        }
        
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
                        .Select(Value.GetFriendlyName)
                        .JoinStrings(" or ") + " value";
                }
                else
                {
                    typeReturn = "literal value depending on your input";
                }

                sb.AppendLine();
                sb.AppendLine($"Returns a {typeReturn}.");
                break;
            }
            case IReturningMethod<CollectionValue>:
                sb.AppendLine();
                sb.AppendLine("Returns a collection of values.");
                break;
            case IReturningMethod<PlayerValue>:
                sb.AppendLine();
                sb.AppendLine("Returns a player value.");
                break;
            case IReferenceReturningMethod refMethod:
                sb.AppendLine();
                sb.AppendLine($"Returns a reference to {refMethod.ReturnType.AccurateName} object.");
                break;
            case IReturningMethod ret:
            {
                string typeReturn;
                if (ret.Returns.AreKnown(out var returnTypes))
                {
                    typeReturn = returnTypes
                        .Select(Value.GetFriendlyName)
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
                sb.AppendLine($" - Default value/behavior: {defVal.StringRep ?? defVal.Value?.ToString() ?? "<unknown>"}");
                sb.AppendLine("   (if needed, you can skip providing this argument by using '_' character)");
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
    }
}