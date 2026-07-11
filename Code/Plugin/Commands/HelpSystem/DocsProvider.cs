using System.Reflection;
using System.Text;
using CommandSystem;
using LabApi.Events.Arguments.Interfaces;
using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Contexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.FlagSystem.Flags;
using SER.Code.Helpers;
using SER.Code.MethodSystem;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.MethodSystem.BaseMethods.Interfaces;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;
using SER.Code.Plugin.Commands.Interfaces;
using SER.Code.ScriptSystem;
using SER.Code.ScriptSystem.Structures;
using SER.Code.TokenSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;
using SER.Code.ValueSystem.PropertySystem;
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
        [HelpOption.Properties] = GetPropertiesHelpPage,
        [HelpOption.Flags] = GetFlagHelpPage,
        [HelpOption.Keywords] = GetKeywordHelpPage
    };

    public static bool GetGeneralOutput(ArraySegment<string> args, out string response)
    {
        var arg = args.Array?[args.Offset].ToLowerInvariant() 
                  ?? throw new Exception("argument provided in invalid format");
        
        if (Enum.TryParse(arg, true, out HelpOption option))
        {
            if (option == HelpOption.Properties && args.Count > 1)
            {
                return GetPropertiesAdvanced(args, out response);
            }

            if (option == HelpOption.Methods && args.Count > 1 && args.Array?[args.Offset + 1] == "essential")
            {
                response = GetMethodList(true);
                return true;
            }
            
            if (!GeneralOptions.TryGetValue(option, out var func))
            {
                throw new AndrzejFuckedUpException($"Option {option} was not added to the help system.");
            }
            
            response = func();
            return true;
        }
        
        if (arg == "properties" && args.Count > 1)
        {
            return GetPropertiesForType(args.Array[args.Offset + 1], out response);
        }
        
        var keyword = ContextableKeywordToken.KeywordContextTypes
            .Select(kType => kType.CreateInstance<IKeywordContext>())
            .FirstOrDefault(keyword => keyword.KeywordName == arg);
        
        if (keyword is not null)
        {
            response = GetKeywordInfo(keyword);
            return true;
        }
        
        var enumType = EnumIndex.GetAllEnums().FirstOrDefault(e => e.Name.ToLowerInvariant() == arg);
        if (enumType is not null)
        {
            response = GetEnum(enumType);
            return true;
        }
        
        var ev = EventSystem.EventHandler.AvailableEvents
            .FirstOrDefault(e => e.Name.ToLowerInvariant() == arg);
        if (ev is not null)
        {
            response = GetEventInfo(ev);
            return true;
        }
        
        var method = MethodIndex.GetMethods()
            .FirstOrDefault(met => met.Name.ToLowerInvariant() == arg);
        if (method is not null)
        {
            response = GetMethodHelp(method);
            return true;
        }
        
        var outsideMethodKvp = MethodIndex.FrameworkDependentMethods
            .Select(kvp => kvp.Value.Select(m => (m, kvp.Key)))
            .Flatten()
            .FirstOrDefault(kvp => kvp.m.Name.ToLowerInvariant() == arg);
        if (outsideMethodKvp is { m: {} outsideMethod, Key: var framework})
        {
            response = GetMethodHelp(outsideMethod, framework);
        }

        var correctFlagName = Flag.FlagInfos.Keys
            .FirstOrDefault(k => k.ToLowerInvariant() == arg);
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
                (1) find the desired option (like '{nameof(HelpOption.Methods).ToLowerInvariant()}')
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

    public static string GetKeywordInfo(IKeywordContext keyword)
    {
        var usageInfo = keyword is IStatementExtender extender
            ? $"""
               --- Usage ---
               This statement can ONLY be used after a statement supporting the "{extender.Extends}" signal!

               # example usage (assuming "somekeyword" supports "{extender.Extends}" signal)
               
               somekeyword
                   # some code
               {keyword.KeywordName} {keyword.Arguments.JoinStrings(" ")}
                   # some other code
               end
               
               """
            : $"""
               --- Usage ---
               {keyword.KeywordName} {keyword.Arguments.JoinStrings(" ")}
               {(keyword is StatementContext ? "\t# some code\nend" : string.Empty)}
               
               """;
        
        var extendableInfo = keyword is IExtendableStatement extendable
            ? $"""
               --- This statement is extendable! ---
               Other statements can be added after this one, provided they support one of the following signal(s):
               {extendable.AllowedSignals.GetFlags().Select(f => $"> {f}").JoinStrings("\n")}
               
               """
            : string.Empty;
        
        // exampel
        var exampel = keyword is { Example: {} e}
            ? $"""
               --- Example Usage ---
               {e}
               
               """
            : string.Empty;
        
        return 
            $"""
            ===== {keyword.KeywordName} keyword =====
            > {keyword.Description}
            
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
            
            """ + ContextableKeywordToken.KeywordContextTypes
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
        var cancellable = typeof(ICancellableEvent).IsAssignableFrom(ev.EventHandlerType.GetGenericArguments().FirstOrDefault());
        var msg = variables.Count > 0 
            ? variables.Aggregate(
                "This event has the following variables attached to it:\n", 
                (current, variable) => current + $"> {variable}\n"
            ) 
            : "This event does not have any variables attached to it.";
        
        return 
             $"""
              Event {ev.Name} is a part of {ev.DeclaringType?.Name ?? "unknown event group"}.
              
              Is cancellable? {cancellable}
              
              {msg}
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
            
            Here are some of the enums used in SER:
            {string.Join("\n", EnumIndex.GetNonReflectedEnums().Select(e => $"> {e.Name}"))}
            """;
    }

    private static Dictionary<string, List<Method>> MethodsByCategory(IEnumerable<Method>? methods = null)
    {
        methods ??= MethodIndex.GetMethods();
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
        
        return methodsByCategory;
    }

    public static string GetMethodList()
    {
        return GetMethodList(false);
    }

    public static string GetMethodList(bool essential)
    {
        const string retsSuffix = " [rets]";

        var allMethods = MethodIndex.GetMethods();
        if (essential)
        {
            allMethods = allMethods.Where(m => m is IEssential).ToArray();
        }

        var sb = new StringBuilder($"Hi! There are {allMethods.Length} {(essential ? "essential " : string.Empty)}methods available for your use!\n");
        sb.AppendLine($"If a method has {retsSuffix.TrimStart()}, it means that this method returns a value.");
        if (essential)
        {
            sb.AppendLine("This list ONLY shows essential methods..");
        }
        sb.AppendLine("If you want to get specific information about a given method, just do 'serhelp <MethodName>'!");

        foreach (var kvp in MethodsByCategory(allMethods).OrderBy(kvp => kvp.Key[0]))
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
            var shownMethods = !essential ? methods : methods.Where(m => m is IEssential).ToList();
            if (shownMethods.Count == 0) continue;
            
            var descDistance = DescDistance(shownMethods);
            
            sb.AppendLine();
            sb.AppendLine($"--- (not accessible) {framework} framework methods ---");
            foreach (var method in shownMethods)
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
            .OfType<PredefinedPlayerVariable>()
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
        if (method is IAdditionalDescription addDesc)
        {
            sb.AppendLine($"> {addDesc.AdditionalDescription}");
        }
        
        if (notLoadedFramework is {} framework)
        {
            sb.AppendLine();
            sb.AppendLine($"This method requires the '{framework}' framework in order to be used.");
            return sb.ToString();
        }

        if (method is IReturningMethod retMethod)
        {
            sb.AppendLine();
            sb.AppendLine($"This method returns {retMethod.Returns}.");
            
            // this is stupid, will have to wait for value system rewrite
            if (retMethod.Returns.AreKnown(out var known))
            {
                var possiblePrefixes = known
                    .Select(t => Value.GetPrefixOfValue(new SingleTypeOfValue(t)))
                    .Distinct()
                    .ToArray();
                
                if (possiblePrefixes.Length == 1)
                {
                    sb.AppendLine($"You can save it to a variable with a '{possiblePrefixes[0]}' prefix.");
                    var addDots = method.ExpectedArguments.Any(arg => arg.MustBeProvided);
                    sb.AppendLine($"{possiblePrefixes[0]}myVariable = {method.Name} {(addDots ? "..." : string.Empty)}");
                }
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
            var optionalArgPrefix = argument.MustBeProvided ? "" : " optional";
            sb.AppendLine($"({index + 1}){optionalArgPrefix} '{argument.Name}' argument");

            if (argument.Description is not null)
            {
                sb.AppendLine($" - Description: {argument.Description}");
            }
            
            sb.AppendLine($" - Expected value: {argument.InputDescription.Replace("\n", "\n\t")}");

            if (argument.DefaultValue is { } defVal)
            {
                sb.AppendLine($" - Default value/behavior: {defVal.StringRep ?? defVal.Value?.ToString() ?? "<unknown>"}");
                sb.AppendLine("   (use '_' character to keep the default)");
            }

            if (argument.ConsumesRemainingValues)
            {
                sb.AppendLine(
                    " - This argument consumes all remaining values; this means that every value provided AFTER " +
                    "this one will also count towards THIS argument's values.");
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

    private static bool TryGetPropsFromValue(Value val, out string response)
    {
        var properties = Value.GetPropertiesOfValue(val.GetType());
        if (properties == null)
        {
            response = $"Value {val.FriendlyName} does not have properties.";
            return false;
        }

        // Special case for collection of references: show both collection and element props
        if (val is CollectionValue { StoredTypes: not null } collection 
            && typeof(ReferenceValue).IsAssignableFrom(collection.StoredTypes))
        {
            var elementProps = Value.GetPropertiesOfValue(collection.StoredTypes);
            if (elementProps != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"> Properties for {val.FriendlyName} (showing both collection and element properties)");
                sb.AppendLine();
                sb.AppendLine("--- Collection Properties ---");
                foreach (var (name, info) in properties.OrderBy(p => p.Key))
                {
                    sb.AppendLine(RenderPropertyLine(name, info));
                }
                sb.AppendLine();
                sb.AppendLine($"--- Element Properties ({Value.GetFriendlyName(collection.StoredTypes)}) ---");
                foreach (var (name, info) in elementProps.OrderBy(p => p.Key))
                {
                    sb.AppendLine(RenderPropertyLine(name, info));
                }
                response = sb.ToString();
                return true;
            }
        }

        response = RenderProperties(val.FriendlyName, properties);
        return true;
    }

    private static string RenderProperties(string typeName, IReadOnlyDictionary<string, IValueWithProperties.PropInfo> props, Type? type = null)
    {
        var sb = new StringBuilder(
            $"> Properties for {typeName} value" 
            + (type is not null ? $" in '{type.Assembly.GetName().Name}' assembly" : "") 
            + "\n");
        
        var sortedProps = props.OrderBy(kvp => kvp.Key).ToList();
        var custom = sortedProps.Where(p => !p.Value.IsReflected).ToList();
        var reflected = sortedProps.Where(p => p.Value.IsReflected).ToList();

        if (reflected.Count > 0)
        {
            sb.AppendLine("\n--- Base properties ---");
            foreach (var (name, info) in reflected)
            {
                sb.AppendLine(RenderPropertyLine(name, info));
            }
        }

        if (custom.Count > 0)
        {
            sb.AppendLine("\n--- Custom SER properties ---");
            foreach (var (name, info) in custom)
            {
                sb.AppendLine(RenderPropertyLine(name, info));
            }
        }
        
        return sb.ToString();
    }

    private static string RenderPropertyLine(string name, IValueWithProperties.PropInfo info)
    {
        var returnTypeFriendlyName = info.ReturnType.ToString();
        return $"> {name} " +
               $"({returnTypeFriendlyName}) " +
               $"{(info.IsSettable ? "[settable] " : "")}" +
               $"{(string.IsNullOrEmpty(info.Description) ? "" : $"- {info.Description}")}";
    }

    public static string GetPropertiesHelpPage()
    {
        var registeredTypes = ReferencePropertyRegistry.GetRegisteredTypes()
            .Select(t => $"> {t.Name}")
            .JoinStrings("\n");

        var playerPropsList = GetTopProperties(new PlayerValue().Properties, "player");
        var collectionPropsList = GetTopProperties(new CollectionValue().Properties, "collection");
        var numberPropsList = GetTopProperties(new NumberValue().Properties, "number");
        var textPropsList = GetTopProperties(new StaticTextValue().Properties, "text");
        var boolPropsList = GetTopProperties(new BoolValue().Properties, "bool");
        var colorPropsList = GetTopProperties(new ColorValue().Properties, "color");
        var durationPropsList = GetTopProperties(new DurationValue().Properties, "duration");

        return
            $$"""
            Properties allow you to access internal data of SER values and SCP:SL objects using the '->' operator.

            Syntax:
            $hp = @player -> hp               - Accesses 'hp' property of a player variable.
            $type = *item -> type             - Accesses 'type' property of a reference variable.
            $key = *json -> someKey           - Accesses 'someKey' from a JSON object.

            Print {@sender -> name}           - You can use {} brackets to contain the expression into a single argument.

            if {@sender -> role} is "ClassD"  - Or use {} when in a condition.

            
            --- Enhanced serhelp properties ---
            You can now inspect properties without knowing the exact type name:
            
            From a global variable:
            > serhelp properties *myVar
            
            From a local variable from a running script:
            > serhelp properties *target @script:round_start
            
            From the return value of a method:
            > serhelp properties run:GetFromMap doors
            
            You can also specify the assembly:
            > serhelp properties Door@LabAPI 

            
            --- Basic SER value properties ---

            Player:
            - {{playerPropsList}}

            Collection:
            - {{collectionPropsList}}

            Number:
            - {{numberPropsList}}

            Text:
            - {{textPropsList}}

            Bool:
            - {{boolPropsList}}

            Color:
            - {{colorPropsList}}

            Duration:
            - {{durationPropsList}}

            
            --- Registered SCP:SL objects ---
            Use 'serhelp properties <objectName>' to see available properties for these types:
            {{registeredTypes}}
            and many more not listed here!
            """;
    }

    private static string GetTopProperties(IReadOnlyDictionary<string, IValueWithProperties.PropInfo> props, string option)
    {
        var list = props.Keys.OrderBy(k => k).Take(5).JoinStrings(", ");
        if (props.Count > 5) list += $", etc. (see 'serhelp properties {option}' for full list)";
        return list;
    }

    public static bool GetPropertiesForType(string typeName, out string response)
    {
        IReadOnlyDictionary<string, IValueWithProperties.PropInfo>? props;

        if (typeName.Equals("player", StringComparison.OrdinalIgnoreCase))
        {
            props = new PlayerValue().Properties;
        }
        else if (typeName.Equals("collection", StringComparison.OrdinalIgnoreCase))
        {
            props = new CollectionValue().Properties;
        }
        else if (typeName.Equals("number", StringComparison.OrdinalIgnoreCase))
        {
            props = new NumberValue().Properties;
        }
        else if (typeName.Equals("text", StringComparison.OrdinalIgnoreCase))
        {
            props = new StaticTextValue().Properties;
        }
        else if (typeName.Equals("bool", StringComparison.OrdinalIgnoreCase) || typeName.Equals("boolean", StringComparison.OrdinalIgnoreCase))
        {
            props = new BoolValue().Properties;
        }
        else if (typeName.Equals("color", StringComparison.OrdinalIgnoreCase))
        {
            props = new ColorValue().Properties;
        }
        else if (typeName.Equals("duration", StringComparison.OrdinalIgnoreCase))
        {
            props = new DurationValue().Properties;
        }
        else
        {
            var types = ReferencePropertyRegistry.GetRegisteredTypes()
                .Where(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase))
                .ToList();
            
            if (types.Count is 0)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var name = assembly.GetName().Name;
                    if (!name.StartsWith("UnityEngine") && !name.StartsWith("LabApi") && !name.StartsWith("NorthwoodLib")
                        && !name.StartsWith("PluginAPI") && !name.StartsWith("Mirror") && !name.StartsWith("SER")
                        && !name.StartsWith("Assembly-CSharp"))
                    {
                        continue;
                    }
                    
                    try
                    {
                        types.AddRange(assembly.GetTypes()
                            .Where(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase)));
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        types.AddRange(e.Types
                            .Where(t => t != null && t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase)));
                    }
                    catch
                    {
                        // Ignore other reflection errors
                    }
                }
            }

            switch (types.Count)
            {
                case 0:
                    response = $"Unknown object type: {typeName}";
                    return false;
                case > 1:
                {
                    var output = new StringBuilder($"Warning! There are {types.Count} defined types with the same name '{typeName}'.\n\n");
                    foreach (var type in types)
                    {
                        output.AppendLine(RenderProperties(typeName, ReferencePropertyRegistry.GetProperties(type), type));
                    }
                
                    response = output.ToString();
                    return true;
                }
                default:
                    props = ReferencePropertyRegistry.GetProperties(types[0]);
                    break;
            }
        }
        
        response = RenderProperties(typeName, props);
        return true;
    }

    // ai
    public static bool GetPropertiesAdvanced(ArraySegment<string> args, out string response)
    {
        var rawInput = string.Join(" ", args.Skip(1));
        var anonymousScript = new Script { Name = ScriptName.CreateUnsafe("HelpAnonymous"), Content = string.Empty, Executor = ServerConsoleExecutor.Instance };

        // 1) Handle Method Execution (run:method ...)
        if (rawInput.StartsWith("run:", StringComparison.OrdinalIgnoreCase))
        {
            var methodLine = rawInput[4..].Trim();
            if (Tokenizer.TokenizeLine(methodLine, anonymousScript, null).HasErrored(out var errorMsg, out var tokens))
            {
                response = $"Error parsing method: {errorMsg}";
                return false;
            }

            if (tokens.Length == 0 || tokens[0] is not MethodToken methodToken)
            {
                response = "The provided input did not resolve to a valid method call.";
                return false;
            }

            var context = (MethodContext)methodToken.GetContext(anonymousScript);
            if (context.Method is not IReturningMethod returningMethod)
            {
                response = $"Method '{context.Method.Name}' does not return a value that can be inspected.";
                return false;
            }

            // Feed remaining tokens to the dispatcher
            foreach (var token in tokens.Skip(1))
            {
                if (context.TryAddToken(token).HasErrored)
                {
                    response = $"Argument error: {context.TryAddToken(token).ErrorMessage}";
                    return false;
                }
            }

            if (context.VerifyCurrentState().HasErrored(out errorMsg))
            {
                response = $"Missing arguments: {errorMsg}";
                return false;
            }

            // Run it synchronously. ReturningMethods are sync (except for SafeScripts wait).
            if (context.Method is SynchronousMethod syncMethod)
            {
                syncMethod.Execute();
                var val = returningMethod.ReturnValue;
                if (val is null)
                {
                    response = "The method was executed but returned no value.";
                    return true;
                }
                return TryGetPropsFromValue(val, out response);
            }

            response = "Only synchronous returning methods can be inspected.";
            return false;
        }

        // 2) Handle Variables and Types via Tokenization
        if (Tokenizer.TokenizeLine(rawInput, anonymousScript, null).HasErrored(out _, out var inputTokens) || inputTokens.Length == 0)
        {
            // Fallback to legacy type lookup if tokenization fails or is empty
            return GetPropertiesForType(args.Array![args.Offset + 1], out response);
        }

        var firstToken = inputTokens[0];
        if (firstToken is VariableToken varToken)
        {
            // Check for @script:name scope
            Script? targetScript = null;
            var scriptParam = inputTokens.FirstOrDefault(t => t.RawRep.StartsWith("@script:", StringComparison.OrdinalIgnoreCase));
            if (scriptParam != null)
            {
                var scriptName = scriptParam.RawRep[8..];
                targetScript = Script.RunningScripts.FirstOrDefault(s => ((string)s.Name).Equals(scriptName, StringComparison.OrdinalIgnoreCase));
                if (targetScript == null)
                {
                    response = $"Script '{scriptName}' is not currently running.\nRunning scripts: " + 
                               (Script.RunningScripts.Any() ? string.Join(", ", Script.RunningScripts.Select(s => s.Name).ToArray()) : "none");
                    return false;
                }
            }

            Value? resolvedValue = null;
            if (targetScript != null)
            {
                var prefix = varToken.RawRep[0];
                var name = varToken.RawRep[1..];
                if (targetScript.LocalVariables.Any(v => v.Prefix == prefix && v.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    resolvedValue = targetScript.LocalVariables.First(v => v.Prefix == prefix && v.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).BaseValue;
                }
                else
                {
                    response = $"Variable '{varToken.RawRep}' was not found in script '{targetScript.Name}'.";
                    return false;
                }
            }
            else if (VariableIndex.TryGetGlobalVariable(varToken.RawRep[0], varToken.RawRep[1..], out var globalVar))
            {
                resolvedValue = globalVar.BaseValue;
            }

            if (resolvedValue != null)
            {
                return TryGetPropsFromValue(resolvedValue, out response);
            }

            response = $"Variable '{varToken.RawRep}' is not defined globally.";
            return false;
        }

        // Default legacy path (or assembly-qualified type)
        return GetPropertiesForType(rawInput, out response);
    }
}