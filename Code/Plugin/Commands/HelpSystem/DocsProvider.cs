using System.Reflection;
using System.Text;
using CommandSystem;
using LabApi.Features.Permissions;
using SER.Code.ContextSystem;
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
    private static string Code(string value) => $"`{value.Replace("`", "\\`")}`";

    private static string BulletList(IEnumerable<string> items) =>
        string.Join("\n", items.Select(item => $"- {item}"));

    private static string Heading(string title, int level = 2) =>
        $"{new string('#', level)} {title}";

    public static string Render(string content, DocsFormat format) =>
        format == DocsFormat.Markdown ? content : RenderPlainText(content);

    public static readonly Dictionary<HelpOption, Func<string>> GeneralOptions = new()
    {
        [HelpOption.Start] = GetStartHelpPage,
        [HelpOption.Methods] = GetMethodIndex,
        [HelpOption.Variables] = GetVariableList,
        [HelpOption.Enums] = GetEnumHelpPage,
        [HelpOption.Events] = GetEventsHelpPage,
        [HelpOption.PmerEvents] = GetPmerEventsHelpPage,
        [HelpOption.UcrEvents] = GetUcrEventsHelpPage,
        [HelpOption.Properties] = GetPropertiesHelpPage,
        [HelpOption.Flags] = GetFlagHelpPage,
        [HelpOption.Keywords] = GetKeywordHelpPage
    };

    public static bool GetGeneralOutput(
        ArraySegment<string> args,
        ICommandSender sender,
        out string response,
        DocsFormat format = DocsFormat.Markdown)
    {
        var result = GetGeneralOutputMarkdown(args, sender, out var markdownResponse);
        response = Render(markdownResponse, format);
        return result;
    }

    private static bool GetGeneralOutputMarkdown(ArraySegment<string> args, ICommandSender sender, out string response)
    {
        var arg = args.Array?[args.Offset].ToLowerInvariant()
                  ?? throw new CoreInvariantException("Help arguments were provided in an invalid format.");

        if (Enum.TryParse(arg, true, out HelpOption option))
        {
            if (option == HelpOption.Properties && args.Count > 1)
            {
                return GetPropertiesAdvanced(args, sender, out response);
            }

            if (option == HelpOption.Methods && args.Count > 1)
            {
                return GetMethodsOutput(args.Array![args.Offset + 1], out response);
            }

            if (!GeneralOptions.TryGetValue(option, out var func))
            {
                throw new CoreInvariantException($"Option {option} was not added to the help system.");
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
            .Concat(EventSystem.EventHandler.AvailablePmerEvents)
            .Concat(EventSystem.EventHandler.AvailableUcrEvents)
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
            return true;
        }

        var correctFlagName = Flag.FlagInfos.Keys
            .FirstOrDefault(k => k.ToLowerInvariant() == arg);
        if (correctFlagName is not null)
        {
            response = GetFlagInfo(correctFlagName);
            return true;
        }

        var suggestion = GetClosestHelpName(arg);
        response = $"There is no '{arg}' help topic." +
                   (suggestion is null ? string.Empty : $" Did you mean 'serhelp {suggestion}'?");
        return false;
    }

    public static string GetOptionsList(DocsFormat format = DocsFormat.Markdown)
    {
        return Render(GetOptionsListMarkdown(), format);
    }

    private static string GetOptionsListMarkdown()
    {
        return $"""
                # SER help

                New here? Run {Code("serhelp start")}.
                For a compact method list, run {Code("serhelp methods essential")}.
                For details about one item, run {Code("serhelp <name>")}, for example {Code("serhelp Print")}.

                ## Help topics

                {BulletList(Enum.GetValues(typeof(HelpOption)).Cast<HelpOption>()
                    .Select(o => Code(o.ToString().LowerFirst())))}


                ## Other commands

                {BulletList(Assembly.GetExecutingAssembly().GetTypes()
                    .Where(t => typeof(ICommand).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract && t != typeof(HelpCommand))
                    .Select(Activator.CreateInstance)
                    .Cast<ICommand>()
                    .Where(c => !string.IsNullOrEmpty(c.Command))
                    .Select(c
                        => $"{Code(c.Command)} (permission: {(c as IUsePermissions)?.Permission ?? "not required"})" +
                           $"{(string.IsNullOrEmpty(c.Description) ? string.Empty : $" — {c.Description}")}"))}
                """;
    }

    public static string GetStartHelpPage()
    {
        return $"""
                # Your first SER script

                1. Script directory:
                   {Code(FileSystem.FileSystem.MainDirPath)}

                2. Create 'hello.ser' there with:
                ```ser
                Print "Hello from SER!"
                ```

                   Use .ser when your host allows unknown file types. Use 'hello.txt' as
                   a compatibility format when its file manager blocks .ser. Both behave identically.

                3. Run:
                ```text
                serrun hello
                ```

                   serrun discovers or reloads the requested file before running it.
                   Round restart reloads all scripts; 'serreload' does the same on demand.

                4. Diagnose files with:
                   {Code("serstatus")}

                Need examples? Run 'serexamples'. Generated files start with '#', so they
                are disabled. Copy one or remove the leading '#', then run 'serreload'.

                Script names are global: only one file with a given base name may exist,
                even when the files are in different folders.
                """;
    }

    private static string GetMethodIndex()
    {
        var methods = MethodIndex.GetMethods();
        var categories = MethodsByCategory(methods)
            .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .Select(pair => $"- {Code(pair.Key)} — {pair.Value.Count} methods");

        return $"""
                SER currently exposes {methods.Length} methods.

                Start with:
                {Code("serhelp methods essential")}

                Browse a category with:
                {Code("serhelp methods <category>")}

                Show the complete list with:
                {Code("serhelp methods all")}

                ## Categories

                {string.Join("\n", categories.Select(category => $"- {category}"))}
                """;
    }

    private static bool GetMethodsOutput(string selector, out string response)
    {
        if (selector.Equals("essential", StringComparison.OrdinalIgnoreCase))
        {
            response = GetMethodList(true);
            return true;
        }

        if (selector.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            response = GetMethodList(false);
            return true;
        }

        var category = MethodsByCategory()
            .FirstOrDefault(pair => pair.Key.Equals(selector, StringComparison.OrdinalIgnoreCase));
        if (category.Value is null)
        {
            response = $"Unknown method category '{selector}'. Run 'serhelp methods' to list categories.";
            return false;
        }

        response = $"{Heading($"{category.Key} methods")}\n\n" +
                   string.Join("\n", category.Value
                       .OrderBy(method => method.Name, StringComparer.OrdinalIgnoreCase)
                       .Select(method => $"- {Code(method.Name)} — {method.Description}")) +
                   $"\n\nUse {Code("serhelp <method name>")} for arguments and return values.";
        return true;
    }

    private static string? GetClosestHelpName(string input)
    {
        var candidates = Enum.GetNames(typeof(HelpOption))
            .Select(name => name.ToLowerInvariant())
            .Concat(ContextableKeywordToken.KeywordContextTypes
                .Select(type => type.CreateInstance<IKeywordContext>().KeywordName))
            .Concat(EnumIndex.GetAllEnums().Select(type => type.Name))
            .Concat(EventSystem.EventHandler.AvailableEvents
                .Concat(EventSystem.EventHandler.AvailablePmerEvents)
                .Concat(EventSystem.EventHandler.AvailableUcrEvents)
                .Select(info => info.Name))
            .Concat(MethodIndex.GetMethods().Select(method => method.Name))
            .Concat(MethodIndex.FrameworkDependentMethods.Values
                .SelectMany(methods => methods)
                .Select(method => method.Name))
            .Concat(Flag.FlagInfos.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(name => new { Name = name, Score = Contexter.GetDiceCoefficient(input, name) })
            .OrderByDescending(candidate => candidate.Score)
            .FirstOrDefault();

        return candidates is { Score: >= 0.35 } ? candidates.Name : null;
    }

    public static string GetKeywordInfo(IKeywordContext keyword)
    {
        var usageInfo = keyword is IStatementExtender extender
            ? $"""
               {Heading("Usage")}
               This statement can only be used after a statement supporting the {Code(extender.Extends.ToString())} signal.

               ```ser
               somekeyword
                   # some code
               {keyword.Usage}
                   # some other code
               end
               ```
               """
            : $"""
               {Heading("Usage")}
               ```ser
               {keyword.Usage}
               {(keyword is StatementContext ? "    # some code\nend" : string.Empty)}
               ```
               """;

        var extendableInfo = keyword is IExtendableStatement extendable
            ? $"""
               {Heading("Signals")}
               **This statement is extendable.** Other statements can be added after this one when they support one of these signals:
               {BulletList(extendable.AllowedSignals.GetFlags().Select(f => Code(f.ToString())))}

               """
            : string.Empty;

        // exampel
        var exampel = keyword is { Example: {} e}
            ? $"""
               {Heading("Example")}
               ```ser
               {e}
               ```
               """
            : string.Empty;

        return
            $"""
            {Heading($"{Code(keyword.KeywordName)} keyword", 1)}
            {keyword.Description}

            {RenderContextArguments(keyword)}

            {usageInfo}
            {extendableInfo}
            {exampel}
            """;
    }

    private static string RenderContextArguments(IKeywordContext keyword)
    {
        if (keyword.Arguments.Length == 0) return string.Empty;

        var lines = keyword.Arguments.Select(argument =>
            $"- **{Code(argument.Syntax)}**{(argument.IsOptional ? " (optional)" : "")} — " +
            $"{argument.Description} Expected: {argument.InputDescription}" +
            (argument.ConsumesRemainingValues ? " This argument accepts all remaining values." : string.Empty));

        return $"{Heading("Arguments")}\n{BulletList(lines)}\n";
    }

    public static string GetKeywordHelpPage()
    {
        return
            $"""
            Keywords alter how the script behaves, not by changing someones role, but the internal script execution.
            They can range from simple things from stopping the script to handling advanced logic.

            Keywords are written as lowercase words, like {Code("stop")} and {Code("if")}.

            Some keywords also have an ability to have instructions inside their "body", making them statements!
            These statements control how the methods inside their body are executed.

            ## Available keywords
            Each keyword can be searched with {Code("serhelp keywordName")}.

            """ + ContextableKeywordToken.KeywordContextTypes
                .Select(t => t.CreateInstance<IKeywordContext>())
                .Select(k => $"- {Code(k.KeywordName)}")
                .JoinStrings("\n");
    }

    public static string GetFlagHelpPage()
    {
        var flags = Flag.FlagInfos.Keys
            .Select(f => $"- {Code(f)}")
            .JoinStrings("\n");

        return
            $"""
            Flags are a way to change script behavior depending on your needs.

            ## Usage
            ```ser
            !-- SomeFlag argValue1 argValue2
            -- customFlagArgument "some value"
            ```

            Flags should be used at the top of the script.

            ## Available flags
            For more information, use {Code("serhelp flagName")}.
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
             {Heading(Code(flagName), 1)}
             {flag.Description}

             ## Usage
             ```ser
             !-- {flagName} {inlineArgumentUsage}
             {argumentsUsage}
             ```

             {(argDesc.Length > 0 ? Heading("Arguments") : "")}
             {argDesc}
             """;
    }

    public static string GetEventInfo(EventInfo ev)
    {
        var variables = EventSystem.EventHandler.GetMimicVariableInfo(ev);
        var eventArgsType = ev.EventHandlerType.GetGenericArguments().FirstOrDefault();
        var cancellable = EventSystem.EventHandler.IsCancellableEvent(ev);
        string result = "This event has the following variables attached to it:\n";
        foreach (var variable in variables)
            result = result + $"- {Code(variable.Display)}" + (string.IsNullOrWhiteSpace(variable.Description)
                ? "\n"
                : $" - {variable.Description}\n");
        var msg = variables.Count > 0
            ? result
            : "This event does not have any variables attached to it.";

        var eventDocumentation = EventSystem.EventHandler.GetEventDescription(ev);
        var eventArgsDocumentation = eventArgsType is null
            ? null
            : XmlDocReader.GetDocumentation(eventArgsType);

        return
             $"""
              {Heading($"{Code(ev.Name)} event", 1)}
              **Group:** {Code(ev.DeclaringType?.Name ?? "unknown event group")}
              {(string.IsNullOrWhiteSpace(eventDocumentation) ? "" : $"\n{eventDocumentation}\n")}
              {(eventArgsType is null ? "" : $"**Event data type:** {Code(eventArgsType.AccurateName)}" +
                  (string.IsNullOrWhiteSpace(eventArgsDocumentation) ? "" : $" - {eventArgsDocumentation}") + "\n")}

              **Cancellable:** {cancellable}

              {msg}
              """;
    }

    public static string GetEventsHelpPage()
    {
        var sb = new StringBuilder();

        foreach (var category in EventSystem.EventHandler.AvailableEvents.Select(ev => ev.DeclaringType).ToHashSet().OfType<Type>())
        {
            sb.AppendLine(Heading(category.Name));
            if (XmlDocReader.GetDocumentation(category) is { Length: > 0 } categoryDocumentation)
                sb.AppendLine(categoryDocumentation);
            sb.AppendLine(BulletList(EventSystem.EventHandler.AvailableEvents
                .Where(ev => ev.DeclaringType == category)
                .Select(ev => Code(ev.Name))));
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

            ## Available events
            {sb}
            """;
    }

    public static string GetPmerEventsHelpPage()
    {
        if (EventSystem.EventHandler.AvailablePmerEvents.Count == 0)
        {
            return
                """
                ProjectMER is not installed or did not expose any supported events.
                The OnPMER flag becomes available for binding when ProjectMER is loaded.
                """;
        }

        var sb = new StringBuilder();
        foreach (var category in EventSystem.EventHandler.AvailablePmerEvents
                     .Select(ev => ev.DeclaringType)
                     .ToHashSet()
                     .OfType<Type>())
        {
            sb.AppendLine(Heading(category.Name));
            if (XmlDocReader.GetDocumentation(category) is { Length: > 0 } categoryDocumentation)
                sb.AppendLine(categoryDocumentation);
            sb.AppendLine(BulletList(EventSystem.EventHandler.AvailablePmerEvents
                .Where(ev => ev.DeclaringType == category)
                .Select(ev => Code(ev.Name))));
        }

        return
            $"""
             ProjectMER events are signals exposed by the optional ProjectMER plugin.
             Use `!-- OnPMER EventName` to run a script when one occurs.

             Event properties are exposed as ev variables. Use `serhelp <eventName>`
             to inspect them and `-- require` to skip execution when selected variables are absent.

             ## Available ProjectMER events
             {sb}
             """;
    }

    public static string GetUcrEventsHelpPage()
    {
        if (EventSystem.EventHandler.AvailableUcrEvents.Count == 0)
        {
            return
                """
                UncomplicatedCustomRoles 9.6.0 or newer is not installed or did not expose any supported events.
                The OnUCR flag becomes available for binding when a supported UCR version is loaded.
                """;
        }

        var sb = new StringBuilder();
        foreach (var category in EventSystem.EventHandler.AvailableUcrEvents
                     .Select(ev => ev.DeclaringType)
                     .ToHashSet()
                     .OfType<Type>())
        {
            sb.AppendLine(Heading(category.Name));
            if (XmlDocReader.GetDocumentation(category) is { Length: > 0 } categoryDocumentation)
                sb.AppendLine(categoryDocumentation);
            sb.AppendLine(BulletList(EventSystem.EventHandler.AvailableUcrEvents
                .Where(ev => ev.DeclaringType == category)
                .Select(ev => Code(ev.Name))));
        }

        return
            $"""
             UCR events tell scripts when a custom role is registered, spawned, or removed.
             Use `!-- OnUCR EventName` to run a script when one occurs.

             Event values are exposed as ev variables. Use `serhelp <eventName>`
             to inspect them and `-- require` to skip execution when selected values are absent.
             Returning false from Registering or Spawning stops that UCR action.

             ## Available UCR events
             {sb}
             """;
    }

    public static string GetEnum(Type enumType)
    {
        var enumDocumentation = XmlDocReader.GetDocumentation(enumType);
        var values = enumType
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.GetCustomAttribute<ObsoleteAttribute>() is null)
            .Select(field =>
            {
                var documentation = XmlDocReader.GetDocumentation(field);
                return $"- {Code(field.Name)}" +
                       (string.IsNullOrWhiteSpace(documentation) ? "" : $" - {documentation}");
            });

        return $"""
                {Heading($"{Code(enumType.Name)} enum", 1)}
                This enum has the following values:
                {(string.IsNullOrWhiteSpace(enumDocumentation) ? "" : $"\n{enumDocumentation}")}
                {string.Join("\n", values)}
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
            {string.Join("\n", EnumIndex.GetNonReflectedEnums().Select(e => $"- {Code(e.Name)}"))}
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
            sb.AppendLine();
            sb.AppendLine(Heading($"{kvp.Key} methods"));
            foreach (var method in kvp.Value)
            {
                sb.AppendLine(GetFormatted(method));
            }
        }

        foreach (var (framework, methods) in MethodIndex.FrameworkDependentMethods
                     .Where(kvp => FrameworkBridge.Found.All(fb => fb.Type != kvp.Key)))
        {
            sb.AppendLine();
            sb.AppendLine($"- **{framework} framework** (not installed) can add {methods.Count} more methods");
        }

        return sb.ToString();

        string GetFormatted(Method method)
        {
            var name = method.Name;
            if (method is ReturningMethod)
            {
                name += retsSuffix;
            }

            return $"- {Code(name)} — {method.Description}";
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
            sb.AppendLine(Heading($"{category ?? "Other"} variables"));
            foreach (var var in allVars.Where(var => var.Category == category))
            {
            sb.AppendLine($"- {Code($"@{var.Name}")}");
            }
        }

        return sb.ToString();
    }

    public static string GetMethodHelp(Method method, FrameworkBridge.Type? notLoadedFramework = null)
    {
        var sb = new StringBuilder($"{Heading(Code(method.Name), 1)}\n\n");

        sb.AppendLine(method.Description);
        if (method is IAdditionalDescription addDesc)
        {
            sb.AppendLine(addDesc.AdditionalDescription);
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

            // This fallback can be removed when the value system is redesigned.
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
                    sb.AppendLine($"```ser\n{possiblePrefixes[0]}myVariable = {method.Name} {(addDots ? "..." : string.Empty)}\n```");
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
            sb.AppendLine($"- **{argument.Name}**{optionalArgPrefix} argument");

            if (argument.Description is not null)
            {
                sb.AppendLine($"  - **Description:** {argument.Description}");
            }

            sb.AppendLine($"  - **Expected value:** {argument.InputDescription.Replace("\n", "\n    ")}");

            if (argument.DefaultValue is { } defVal)
            {
                sb.AppendLine($"  - **Default value/behavior:** {defVal.StringRep ?? defVal.Value?.ToString() ?? "<unknown>"}");
                sb.AppendLine("    (use `_` to keep the default)");
            }

            if (argument.ConsumesRemainingValues)
            {
                sb.AppendLine(
                    "  - This argument consumes all remaining values; every value provided AFTER " +
                    "this one will also count towards THIS argument's values.");
            }
        }

        if (method is ICanError errorMethod)
        {
            sb.AppendLine();
            sb.AppendLine("This method defines custom errors:");
            sb.AppendLine(errorMethod.ErrorReasons.Select(e => $"- {e}").JoinStrings("\n"));
        }

        return sb.ToString();
    }

    private static bool TryGetPropsFromValue(Value val, out string response)
    {
        response = string.Empty;
        var properties = Value.GetPropertiesOfValue(val.GetType());
        if (properties == null)
        {
            response = $"Value {val.FriendlyName} does not have properties.";
            return false;
        }

        // Special case for shell types
        if (val is ReferenceValue { Value: IFrameworkTypeShell shell })
        {
            var innerType = shell.Object.GetType();
            if (Value.GetPropertiesOfValue(innerType) is not { } innerProps)
            {
                innerProps = ReferencePropertyRegistry.GetProperties(innerType);
            }

            response = RenderProperties(innerType.AccurateName, innerProps, innerType);
            return true;
        }

        // Special case for collection of references: show both collection and element props
        if (val is CollectionValue { StoredTypes: not null } collection
            && typeof(ReferenceValue).IsAssignableFrom(collection.StoredTypes))
        {
            var elementProps = Value.GetPropertiesOfValue(collection.StoredTypes);
            if (elementProps != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"**Properties for {val.FriendlyName}** (showing both collection and element properties)");
                sb.AppendLine();
                sb.AppendLine(Heading("Collection properties"));
                foreach (var (name, info) in properties.OrderBy(p => p.Key))
                {
                    sb.AppendLine(RenderPropertyLine(name, info));
                }
                sb.AppendLine();
                sb.AppendLine(Heading($"Element properties ({Value.GetFriendlyName(collection.StoredTypes)})"));
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
            $"**Properties for {typeName} value**"
            + (type is not null ? $" in '{type.Assembly.GetName().Name}' assembly" : "")
            + "\n");

        if (type is not null && XmlDocReader.GetDocumentation(type) is { Length: > 0 } typeDocumentation)
        {
            sb.AppendLine(typeDocumentation);
        }

        var sortedProps = props.OrderBy(kvp => kvp.Key).ToList();
        var custom = sortedProps.Where(p => !p.Value.IsReflected).ToList();
        var reflected = sortedProps.Where(p => p.Value.IsReflected).ToList();

        if (reflected.Count > 0)
        {
            sb.AppendLine($"\n{Heading("Base properties")}");
            foreach (var (name, info) in reflected)
            {
                sb.AppendLine(RenderPropertyLine(name, info));
            }
        }

        if (custom.Count > 0)
        {
            sb.AppendLine($"\n{Heading("Custom SER properties")}");
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
        return $"- {Code(name)} " +
               $"({Code(returnTypeFriendlyName)}) " +
               $"{(info.IsSettable ? "[settable] " : "")}" +
               $"{(string.IsNullOrEmpty(info.Description) ? "" : $"- {info.Description}")}";
    }

    public static string GetPropertiesHelpPage()
    {
        var registeredTypes = ReferencePropertyRegistry.GetRegisteredTypes()
            .Select(t => $"- {Code(t.Name)}")
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


            ## Enhanced property lookup
            You can now inspect properties without knowing the exact type name:

            From a global variable:
            > serhelp properties *myVar

            From a local variable from a running script:
            > serhelp properties *target script:round_start

            From the return value of a method:
            > serhelp properties run:GetFromMap doors

            You can also specify the assembly:
            > serhelp properties Door@LabAPI


            ## Basic SER value properties

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


            ## Registered SCP:SL objects
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
        Type? reflectedType = null;

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
                    reflectedType = types[0];
                    props = ReferencePropertyRegistry.GetProperties(reflectedType);
                    break;
            }
        }

        response = RenderProperties(typeName, props, reflectedType);
        return true;
    }

    private static string RenderPlainText(string content)
    {
        var lines = content.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        var output = new StringBuilder();
        var inCodeBlock = false;

        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index];
            var trimmed = line.Trim();

            if (trimmed.StartsWith("```", StringComparison.Ordinal))
            {
                inCodeBlock = !inCodeBlock;
                continue;
            }

            if (inCodeBlock)
            {
                output.AppendLine(line);
                continue;
            }

            if (trimmed.Length == 0)
            {
                output.AppendLine();
                continue;
            }

            var heading = trimmed.TakeWhile(c => c == '#').Count();
            if (heading is > 0 and <= 6 && trimmed.Length > heading && char.IsWhiteSpace(trimmed[heading]))
            {
                var title = StripInlineMarkdown(trimmed[(heading + 1)..].Trim());
                output.AppendLine(title);
                output.AppendLine(new string(heading == 1 ? '=' : '-', Math.Max(3, title.Length)));
                continue;
            }

            output.AppendLine(StripInlineMarkdown(line));
        }

        return output.ToString().TrimEnd();
    }

    private static string StripInlineMarkdown(string line)
    {
        line = line.Replace("\\`", "`");
        line = System.Text.RegularExpressions.Regex.Replace(line, @"`([^`]*)`", "$1");
        line = System.Text.RegularExpressions.Regex.Replace(line, @"\[([^\]]+)\]\([^\)]+\)", "$1");
        line = line.Replace("**", string.Empty).Replace("__", string.Empty);
        line = System.Text.RegularExpressions.Regex.Replace(line, @"(?<!\w)[*_](.*?)[*_](?!\w)", "$1");
        line = System.Text.RegularExpressions.Regex.Replace(line, @"^\s*>\s?", "");
        return line;
    }

    // ai
    public static bool GetPropertiesAdvanced(
        ArraySegment<string> args,
        ICommandSender sender,
        out string response)
    {
        var rawInput = string.Join(" ", args.Skip(1));
        var executor = ScriptExecutor.Get(sender);

        // 1) Handle Method Execution (run:method ...)
        if (rawInput.StartsWith("run:", StringComparison.OrdinalIgnoreCase))
        {
            if (!sender.HasAnyPermission(MethodCommand.RunPermission))
            {
                response = "You do not have permission to run scripts.";
                return false;
            }

            var methodLine = rawInput[4..].Trim();
            var methodScript = new Script
            {
                Name = ScriptName.CreateUnsafe("HelpAnonymous"),
                Content = methodLine,
                Executor = executor
            };

            if (methodScript.Compile().HasErrored(out var compileError))
            {
                response = $"Error parsing method: {compileError}";
                return false;
            }

            if (!methodScript.IsSingleSynchronousReturningMethod)
            {
                response = "Only a single synchronous returning method can be inspected.";
                return false;
            }

            if (methodScript.RunSingleSynchronousReturningMethod(RunReason.BaseCommand)
                .HasErrored(out var runtimeError, out var value))
            {
                response = runtimeError;
                return false;
            }

            return TryGetPropsFromValue(value, out response);
        }

        // 2) Handle Variables and Types via Tokenization
        var anonymousScript = new Script
        {
            Name = ScriptName.CreateUnsafe("HelpAnonymous"),
            Content = string.Empty,
            Executor = executor
        };
        if (Tokenizer.TokenizeLine(rawInput, anonymousScript, null).HasErrored(out _, out var inputTokens) || inputTokens.Length == 0)
        {
            // Fallback to legacy type lookup if tokenization fails or is empty
            return GetPropertiesForType(args.Array![args.Offset + 1], out response);
        }

        var firstToken = inputTokens[0];
        if (firstToken is VariableToken varToken)
        {
            // Check for script:name scope
            Script? targetScript = null;
            var scriptParam = inputTokens.FirstOrDefault(t => t.RawRep.StartsWith("script:", StringComparison.OrdinalIgnoreCase));
            if (scriptParam != null)
            {
                var scriptName = scriptParam.RawRep["script:".Length..];
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
