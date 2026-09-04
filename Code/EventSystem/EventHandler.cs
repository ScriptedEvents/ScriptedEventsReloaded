using System.Linq.Expressions;
using System.Reflection;
using LabApi.Events.Arguments.Interfaces;
using LabApi.Loader;
using PlayerStatsSystem;
using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.Helpers.ResultSystem;
using SER.Code.Integrations.Mer;
using SER.Code.Integrations.Ucr;
using SER.Code.ScriptSystem;
using SER.Code.ScriptSystem.Structures;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;
using SER.Code.VariableSystem.Bases;
using DamageHandlerBase = PlayerStatsSystem.DamageHandlerBase;

namespace SER.Code.EventSystem;

public static class EventHandler
{
    private enum EventSource
    {
        LabApi,
        ProjectMer,
        Ucr
    }

    private readonly record struct EventKey(EventSource Source, string Name);

    public sealed record EventVariableInfo(string Name, string Type, string? Description)
    {
        public string Display => $"{Name} ({Type})";
    }

    private static readonly List<Action> UnsubscribeActions = [];
    private static readonly Dictionary<EventKey, List<Action<object?, Variable[]>>> OnEventActions = [];
    private static readonly Dictionary<(ScriptName Script, EventKey Event), Action<object?, Variable[]>> ScriptEventActions = [];
    private static readonly HashSet<string> DisabledEvents = [];
    private static readonly Dictionary<string, string> UcrEventDescriptions = new(StringComparer.Ordinal)
    {
        ["Registering"] = "Runs before UCR registers a custom role. Use `IsAllowed false` to stop the registration.",
        ["Registered"] = "Runs after UCR registers a custom role.",
        ["Unregistered"] = "Runs after UCR unregisters a custom role.",
        ["Spawning"] = "Runs before UCR gives a player a custom role. Use `IsAllowed false` to stop the spawn.",
        ["Spawned"] = "Runs after UCR gives a player a custom role.",
        ["Removed"] = "Runs after UCR removes a custom role from a player."
    };
    private static readonly Dictionary<string, string> UcrVariableDescriptions = new(StringComparer.Ordinal)
    {
        ["Role"] = "The UCR role involved in this event.",
        ["Player"] = "The player involved in this event.",
        ["Instance"] = "The UCR role instance involved in this event.",
        ["IsAllowed"] = "Whether UCR will continue the action."
    };
    public static List<EventInfo> AvailableEvents = [];
    public static List<EventInfo> AvailablePmerEvents = [];
    public static List<EventInfo> AvailableUcrEvents = [];
    public static readonly HashSet<string> RegisteredHandlers = [];
    public static readonly HashSet<string> BindedEvents = [];
    public static readonly HashSet<string> BindedPmerEvents = [];
    public static readonly HashSet<string> BindedUcrEvents = [];
    
    public static void Initialize()
    {
        Clear();
        AvailableEvents = typeof(PluginLoader).Assembly.GetTypes()
            .Where(t => t.FullName?.Equals($"LabApi.Events.Handlers.{t.Name}") is true)
            .Select(t => t.GetEvents(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public 
                                     | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).ToList())
            .Flatten().ToList();

        AvailablePmerEvents = GetOptionalProjectMerEvents(loadOptionalAssembly: false);
        AvailableUcrEvents = GetOptionalUcrEvents(loadOptionalAssembly: false);
    }

    public static void LoadOptionalProjectMerEventsForTooling()
    {
        AvailablePmerEvents = GetOptionalProjectMerEvents(loadOptionalAssembly: true);
    }

    public static void LoadOptionalUcrEventsForTooling()
    {
        AvailableUcrEvents = GetOptionalUcrEvents(loadOptionalAssembly: true);
    }

    private static List<EventInfo> GetOptionalProjectMerEvents(bool loadOptionalAssembly)
    {
        Assembly? merAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(assembly => assembly.GetName().Name == "ProjectMER");
        if (merAssembly is null && loadOptionalAssembly)
        {
            try
            {
                merAssembly = Assembly.Load("ProjectMER");
            }
            catch (FileNotFoundException)
            {
                // ProjectMER is optional. Runtime discovery simply remains empty.
            }
            catch (FileLoadException)
            {
                // ProjectMER is optional. Runtime discovery simply remains empty.
            }
            catch (BadImageFormatException)
            {
                // ProjectMER is optional. Runtime discovery simply remains empty.
            }
        }

        if (merAssembly is null)
            return [];

        Type? schematicHandler = merAssembly.GetType("ProjectMER.Events.Handlers.Schematic");
        if (schematicHandler is null)
            return [];

        return schematicHandler.GetEvents(BindingFlags.Public | BindingFlags.Static).ToList();
    }

    private static List<EventInfo> GetOptionalUcrEvents(bool loadOptionalAssembly)
    {
        Assembly? ucrAssembly = GetOptionalAssembly("UncomplicatedCustomRoles", loadOptionalAssembly);
        Type? eventHandler = ucrAssembly?.GetType("UncomplicatedCustomRoles.API.Events.CustomRoleEvents");
        return eventHandler?.GetEvents(BindingFlags.Public | BindingFlags.Static).ToList() ?? [];
    }

    private static Assembly? GetOptionalAssembly(string assemblyName, bool loadOptionalAssembly)
    {
        Assembly? assembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(candidate => candidate.GetName().Name == assemblyName);
        if (assembly is not null || !loadOptionalAssembly)
            return assembly;

        try
        {
            return Assembly.Load(assemblyName);
        }
        catch (FileNotFoundException)
        {
            return null;
        }
        catch (FileLoadException)
        {
            return null;
        }
        catch (BadImageFormatException)
        {
            return null;
        }
    }
    
    public static void Clear()
    {
        RegisteredHandlers.Clear();
        OnEventActions.Clear();
        ScriptEventActions.Clear();
        foreach (var unsubscribeAction in UnsubscribeActions)
        {
            unsubscribeAction();
        }
        UnsubscribeActions.Clear();
        DisabledEvents.Clear();
        BindedEvents.Clear();
        BindedPmerEvents.Clear();
        BindedUcrEvents.Clear();
        AvailablePmerEvents.Clear();
        AvailableUcrEvents.Clear();
    }

    public static TryGet<bool> DisableEvent(string evName)
    {
        if (GetCancellableEvent(evName).HasErrored(out var error))
        {
            return error;
        }

        if (BindEvent(new EventKey(EventSource.LabApi, evName)).HasErrored(out error))
        {
            return error;
        }

        return DisabledEvents.Add(evName);
    }

    public static TryGet<bool> EnableEvent(string evName)
    {
        if (GetCancellableEvent(evName).HasErrored(out var error))
        {
            return error;
        }

        return DisabledEvents.Remove(evName);
    }

    private static TryGet<EventInfo> GetCancellableEvent(string evName)
    {
        var eventInfo = AvailableEvents.FirstOrDefault(e => e.Name == evName);
        if (eventInfo is null)
        {
            return $"Event '{evName}' does not exist!";
        }

        var eventArgsType = eventInfo.EventHandlerType?.GetGenericArguments().FirstOrDefault();
        if (eventArgsType is null || !typeof(ICancellableEvent).IsAssignableFrom(eventArgsType))
        {
            return $"Event '{evName}' cannot be disabled because it is not cancellable!";
        }

        return eventInfo;
    }
    
    public static Result AddEventHandler(string evName, ScriptName scriptName) 
        => AddEventHandler(new EventKey(EventSource.LabApi, evName), scriptName);

    public static Result AddPmerEventHandler(string evName, ScriptName scriptName)
        => AddEventHandler(new EventKey(EventSource.ProjectMer, evName), scriptName);

    public static Result AddUcrEventHandler(string evName, ScriptName scriptName)
        => AddEventHandler(new EventKey(EventSource.Ucr, evName), scriptName);

    private static Result AddEventHandler(EventKey eventKey, ScriptName scriptName)
    {
        var handlerId = $"'{scriptName}' script";
        if (ScriptEventActions.ContainsKey((scriptName, eventKey)))
        {
            return true;
        }
        
        if (BindEvent(eventKey).HasErrored(out var error))
        {
            return error;
        }
        
        var action = RunScriptOnEvent(scriptName, eventKey);
        ScriptEventActions[(scriptName, eventKey)] = action;
        RegisteredHandlers.Add(handlerId);
        if (OnEventActions.TryGetValue(eventKey, out var actions))
        {
            actions.Add(action);
            return true;
        }
        
        OnEventActions.Add(eventKey, [action]);
        return true;
    }

    public static void RemoveEventHandler(string evName, ScriptName scriptName)
        => RemoveEventHandler(new EventKey(EventSource.LabApi, evName), scriptName);

    public static void RemovePmerEventHandler(string evName, ScriptName scriptName)
        => RemoveEventHandler(new EventKey(EventSource.ProjectMer, evName), scriptName);

    public static void RemoveUcrEventHandler(string evName, ScriptName scriptName)
        => RemoveEventHandler(new EventKey(EventSource.Ucr, evName), scriptName);

    private static void RemoveEventHandler(EventKey eventKey, ScriptName scriptName)
    {
        if (!ScriptEventActions.TryGetValue((scriptName, eventKey), out var action))
        {
            return;
        }

        ScriptEventActions.Remove((scriptName, eventKey));

        if (OnEventActions.TryGetValue(eventKey, out var actions))
        {
            actions.Remove(action);
            if (actions.Count == 0)
            {
                OnEventActions.Remove(eventKey);
            }
        }

        RegisteredHandlers.Remove($"'{scriptName}' script");
    }
    
    public static Result AddEventHandler(string evName, Action<EventArgs?, Variable[]> action, string handlerId)
        => AddExternalEventHandler(evName, (eventArgs, variables) => action(eventArgs as EventArgs, variables), handlerId);

    private static Result AddExternalEventHandler(string evName, Action<object?, Variable[]> action, string handlerId)
    {
        if (RegisteredHandlers.Contains(handlerId))
        {
            return $"{handlerId}' is already registered as an event handler!";
        }
        
        var eventKey = new EventKey(EventSource.LabApi, evName);
        if (BindEvent(eventKey).HasErrored(out var error))
        {
            return error;
        }
        
        RegisteredHandlers.Add(handlerId);
        if (OnEventActions.TryGetValue(eventKey, out var actions))
        {
            actions.Add(action);
        }
        else
        {
            OnEventActions.Add(eventKey, [action]);
        }
        
        return true;
    }

    private static Result BindEvent(EventKey eventKey)
    {
        var availableEvents = eventKey.Source switch
        {
            EventSource.ProjectMer => AvailablePmerEvents,
            EventSource.Ucr => AvailableUcrEvents,
            _ => AvailableEvents
        };
        EventInfo? matchingEventInfo = availableEvents.FirstOrDefault(e => e.Name == eventKey.Name);
        if (matchingEventInfo is null)
        {
            return eventKey.Source switch
            {
                EventSource.ProjectMer when AvailablePmerEvents.Count == 0 =>
                    "ProjectMER is not installed or did not expose any supported events.",
                EventSource.Ucr when AvailableUcrEvents.Count == 0 =>
                    "UncomplicatedCustomRoles 9.6.0 or newer is not installed or did not expose any supported events.",
                _ => $"Event '{eventKey.Name}' does not exist!"
            };
        }

        var boundEvents = eventKey.Source switch
        {
            EventSource.ProjectMer => BindedPmerEvents,
            EventSource.Ucr => BindedUcrEvents,
            _ => BindedEvents
        };
        if (!boundEvents.Add(eventKey.Name))
        {
            // already binded
            return true;
        }
        
        var genericType = matchingEventInfo.EventHandlerType.GetGenericArguments().FirstOrDefault();
        if (genericType is not null)
        {
            BindArgumented(matchingEventInfo, genericType, eventKey);
            return true;
        }
        
        BindNonArgumented(matchingEventInfo, eventKey);
        return true;
    }

    private static Action<object?, Variable[]> RunScriptOnEvent(ScriptName scrName, EventKey eventKey)
    {
        return (ev, variables) =>
        {
            Result rs = $"Failed to run script '{scrName}' connected to event '{eventKey.Name}'";
            Log.Debug($"Running script '{scrName}' for {eventKey.Source} event '{eventKey.Name}'");

            if (Script.CreateByScriptName(scrName, ScriptExecutor.Get()).HasErrored(out var error, out var script))
            {
                Log.CompileError(scrName, rs + error);
                return;
            }

            script.AddLocalVariables(variables);
            script.RunForEvent(
                RunReason.Event,
                eventAllowedChanged: isAllowed => SetEventAllowed(ev, eventKey.Source, isAllowed));
        };
    }

    private static void SetEventAllowed(object? eventArgs, EventSource source, bool isAllowed)
    {
        if (eventArgs is ICancellableEvent cancellable)
        {
            cancellable.IsAllowed = isAllowed;
        }
        else if (eventArgs is not null && source == EventSource.Ucr)
        {
            UcrBridge.TrySetEventAllowed(eventArgs, isAllowed);
        }
    }

    private static void BindNonArgumented(EventInfo eventInfo, EventKey eventKey)
    {
        // Create delegate that captures the event source and name
        var call = Expression.Call(
            typeof(EventHandler).GetMethod(nameof(OnNonArgumentedEvent), BindingFlags.Static | BindingFlags.NonPublic)!,
            Expression.Constant(eventKey));
        var handler = Expression.Lambda(eventInfo.EventHandlerType!, call).Compile();

        // Subscribe
        eventInfo.GetAddMethod(false).Invoke(null!, [handler]);

        // Store unsubscribe action
        UnsubscribeActions.Add(() => eventInfo.GetRemoveMethod(false).Invoke(null!, [handler]));
    }

    private static void BindArgumented(EventInfo eventInfo, Type generic, EventKey eventKey)
    {
        // We'll build (T ev) => OnArgumentedEvent(eventKey, ev)
        var evParam = Expression.Parameter(generic, "ev");
        var keyConst = Expression.Constant(eventKey);
        var call = Expression.Call(
            typeof(EventHandler)
                .GetMethod(nameof(OnArgumentedEvent), BindingFlags.Static | BindingFlags.NonPublic)!
                .MakeGenericMethod(generic),
            keyConst,
            evParam
        );

        // Compile the exact delegate type exposed by the optional framework.
        var lambda = Expression.Lambda(eventInfo.EventHandlerType!, call, evParam);
        var handler = lambda.Compile();

        // Subscribe
        eventInfo.GetAddMethod(false).Invoke(null!, [handler]);

        // Store unsubscribe action
        UnsubscribeActions.Add(() => eventInfo.GetRemoveMethod(false).Invoke(null!, [handler]));
    }

    private static void OnNonArgumentedEvent(EventKey eventKey)
    {
        Log.Debug($"[NonArg] {eventKey.Source} event '{eventKey.Name}' triggered.");

        if (eventKey.Source == EventSource.LabApi && DisabledEvents.Contains(eventKey.Name))
            return;

        if (!OnEventActions.TryGetValue(eventKey, out var actions))
            return;

        foreach (var action in actions.ToArray()) action(null, []);
    }

    private static void OnArgumentedEvent<T>(EventKey eventKey, T ev)
    {
        Log.Debug($"[Arg] {eventKey.Source} event '{eventKey.Name}' triggered with {typeof(T).AccurateName}.");

        if (eventKey.Source == EventSource.LabApi &&
            ev is ICancellableEvent cancellable &&
            DisabledEvents.Contains(eventKey.Name))
        {
            cancellable.IsAllowed = false;
            Log.Debug($"Event '{eventKey.Name}' cancelled (disabled).");
            return;
        }

        var variables = GetVariablesFromEvent(ev!, eventKey.Source);
        if (!OnEventActions.TryGetValue(eventKey, out var actions))
        {
            Log.Debug($"Event '{eventKey.Name}' has no scripts connected.");
            return;
        }

        foreach (var action in actions.ToArray()) action(ev, variables);
    }
    
    public static Variable[] GetVariablesFromEvent(EventArgs ev)
        => GetVariablesFromEvent(ev, EventSource.LabApi);

    private static Variable[] GetVariablesFromEvent(object ev, EventSource source)
    {
        List<(object, string, Type)> properties = (
            from prop in ev.GetType().GetProperties()
            where !Attribute.IsDefined(prop, typeof(ObsoleteAttribute))
            let value = prop.GetValue(ev)
            let type = prop.PropertyType
            select (
                value is null ? null : source switch
                {
                    EventSource.ProjectMer => MerBridge.WrapEventValue(value),
                    EventSource.Ucr => UcrBridge.WrapEventValue(value),
                    _ => value
                },
                prop.Name,
                source switch
                {
                    EventSource.ProjectMer => MerBridge.GetEventValueType(type),
                    EventSource.Ucr => UcrBridge.GetEventValueType(type),
                    _ => type
                })
        ).ToList();

        return InternalGetVariablesFromProperties(properties);
    }
    
    public static List<string> GetMimicVariables(EventInfo ev)
        => GetMimicVariableInfo(ev).Select(variable => variable.Display).ToList();

    public static List<EventVariableInfo> GetMimicVariableInfo(EventInfo ev)
    {
        if (ev.EventHandlerType.GetGenericArguments().FirstOrDefault() is not { } genericType)
        {
            return [];
        }

        List<(Type type, string name, string? description)> properties = (
            from prop in genericType.GetProperties()
            where !Attribute.IsDefined(prop, typeof(ObsoleteAttribute))
            let value = prop.PropertyType
            where value is not null
            select (
                IsPmerEvent(ev)
                    ? MerBridge.GetEventValueType(value)
                    : IsUcrEvent(ev)
                        ? UcrBridge.GetEventValueType(value)
                        : value,
                prop.Name,
                GetEventVariableDescription(ev, prop))
        ).ToList();
        
        return GetMimicVariablesForEventHelp(properties);
    }

    public static bool IsPmerEvent(EventInfo eventInfo)
        => eventInfo.DeclaringType?.Assembly.GetName().Name == "ProjectMER";

    public static bool IsUcrEvent(EventInfo eventInfo)
        => eventInfo.DeclaringType?.Assembly.GetName().Name == "UncomplicatedCustomRoles";

    public static bool IsCancellableEvent(EventInfo eventInfo)
    {
        var eventArgsType = eventInfo.EventHandlerType?.GetGenericArguments().FirstOrDefault();
        return eventArgsType is not null &&
               (typeof(ICancellableEvent).IsAssignableFrom(eventArgsType) ||
                IsUcrEvent(eventInfo) && UcrBridge.IsCancellableEvent(eventArgsType));
    }

    public static string? GetEventDescription(EventInfo eventInfo)
    {
        return XmlDocReader.GetDocumentation(eventInfo) is { Length: > 0 } documentation
            ? documentation
            : IsUcrEvent(eventInfo) && UcrEventDescriptions.TryGetValue(eventInfo.Name, out var description)
                ? description
                : null;
    }

    private static string? GetEventVariableDescription(EventInfo eventInfo, PropertyInfo property)
    {
        return XmlDocReader.GetDocumentation(property) is { Length: > 0 } documentation
            ? documentation
            : IsUcrEvent(eventInfo) && UcrVariableDescriptions.TryGetValue(property.Name, out var description)
                ? description
                : null;
    }

    private static Variable[] InternalGetVariablesFromProperties(List<(object value, string name, Type type)> properties)
    {
        List<Variable> variables = [];
        foreach (var (value, name, _) in properties)
        {
            if (value is null) continue;
            variables.Add(Variable.Create(
                $"ev{name[0].ToString().ToUpper()}{name[1..]}", 
                Value.Parse(value))
            );
        }

        return variables.ToArray();
    }
    
    private static List<EventVariableInfo> GetMimicVariablesForEventHelp(
        List<(Type type, string name, string? description)> properties)
    {
        List<EventVariableInfo> variables = [];
        foreach (var (type, name, description) in properties)
        {
            if (type is null) continue;
            var typeOfValue = new SingleTypeOfValue(Value.GuessValueType(type));
            
            // Only StandardDamageHandler inherits from DamageHandlerBase in the game API.
            if (typeOfValue.Is<ReferenceValue<DamageHandlerBase>>())
            {
                typeOfValue = new TypeOfValue<ReferenceValue<StandardDamageHandler>>();
            }
            
            variables.Add(new EventVariableInfo(
                $"{Value.GetPrefixOfValue(typeOfValue)}ev{name}",
                typeOfValue.ToString(),
                description));
        }

        return variables;
    }
}
