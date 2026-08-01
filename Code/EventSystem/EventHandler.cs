using System.Linq.Expressions;
using System.Reflection;
using LabApi.Events;
using LabApi.Events.Arguments.Interfaces;
using LabApi.Loader;
using PlayerStatsSystem;
using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.Helpers.ResultSystem;
using SER.Code.Integrations.Mer;
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
        ProjectMer
    }

    private readonly record struct EventKey(EventSource Source, string Name);

    public sealed record EventVariableInfo(string Name, string Type, string? Description)
    {
        public string Display => $"{Name} ({Type})";
    }

    private static readonly List<Action> UnsubscribeActions = [];
    private static readonly Dictionary<EventKey, List<Action<EventArgs?, Variable[]>>> OnEventActions = [];
    private static readonly Dictionary<(ScriptName Script, EventKey Event), Action<EventArgs?, Variable[]>> ScriptEventActions = [];
    private static readonly HashSet<string> DisabledEvents = [];
    public static List<EventInfo> AvailableEvents = [];
    public static List<EventInfo> AvailablePmerEvents = [];
    public static readonly HashSet<string> RegisteredHandlers = [];
    public static readonly HashSet<string> BindedEvents = [];
    public static readonly HashSet<string> BindedPmerEvents = [];
    
    public static void Initialize()
    {
        Clear();
        AvailableEvents = typeof(PluginLoader).Assembly.GetTypes()
            .Where(t => t.FullName?.Equals($"LabApi.Events.Handlers.{t.Name}") is true)
            .Select(t => t.GetEvents(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public 
                                     | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).ToList())
            .Flatten().ToList();

        AvailablePmerEvents = GetOptionalProjectMerEvents(loadOptionalAssembly: false);
    }

    public static void LoadOptionalProjectMerEventsForTooling()
    {
        AvailablePmerEvents = GetOptionalProjectMerEvents(loadOptionalAssembly: true);
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
        AvailablePmerEvents.Clear();
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
        var availableEvents = eventKey.Source == EventSource.ProjectMer
            ? AvailablePmerEvents
            : AvailableEvents;
        EventInfo? matchingEventInfo = availableEvents.FirstOrDefault(e => e.Name == eventKey.Name);
        if (matchingEventInfo is null)
        {
            return eventKey.Source == EventSource.ProjectMer && AvailablePmerEvents.Count == 0
                ? "ProjectMER is not installed or did not expose any supported events."
                : $"Event '{eventKey.Name}' does not exist!";
        }

        var boundEvents = eventKey.Source == EventSource.ProjectMer
            ? BindedPmerEvents
            : BindedEvents;
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

    private static Action<EventArgs?, Variable[]> RunScriptOnEvent(ScriptName scrName, EventKey eventKey)
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
            var isAllowed = script.RunForEvent(RunReason.Event);
            if (isAllowed.HasValue && ev is ICancellableEvent cancellable1)
                cancellable1.IsAllowed = isAllowed.Value;
        };
    }

    private static void BindNonArgumented(EventInfo eventInfo, EventKey eventKey)
    {
        // Create delegate that captures the event source and name
        LabEventHandler handler = () => OnNonArgumentedEvent(eventKey);

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

        // Compile delegate of correct type: LabEventHandler<T>
        var delegateType = typeof(LabEventHandler<>).MakeGenericType(generic);
        var lambda = Expression.Lambda(delegateType, call, evParam);
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

    private static void OnArgumentedEvent<T>(EventKey eventKey, T ev) where T : EventArgs
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

        var variables = GetVariablesFromEvent(ev, eventKey.Source);
        if (!OnEventActions.TryGetValue(eventKey, out var actions))
        {
            Log.Debug($"Event '{eventKey.Name}' has no scripts connected.");
            return;
        }

        foreach (var action in actions.ToArray()) action(ev, variables);
    }
    
    public static Variable[] GetVariablesFromEvent(EventArgs ev)
        => GetVariablesFromEvent(ev, EventSource.LabApi);

    private static Variable[] GetVariablesFromEvent(EventArgs ev, EventSource source)
    {
        List<(object, string, Type)> properties = (
            from prop in ev.GetType().GetProperties()
            where !Attribute.IsDefined(prop, typeof(ObsoleteAttribute))
            let value = prop.GetValue(ev)
            let type = prop.PropertyType
            select (
                source == EventSource.ProjectMer && value is not null
                    ? MerBridge.WrapEventValue(value)
                    : value,
                prop.Name,
                source == EventSource.ProjectMer
                    ? MerBridge.GetEventValueType(type)
                    : type)
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
                IsPmerEvent(ev) ? MerBridge.GetEventValueType(value) : value,
                prop.Name,
                XmlDocReader.GetDocumentation(prop))
        ).ToList();
        
        return GetMimicVariablesForEventHelp(properties);
    }

    public static bool IsPmerEvent(EventInfo eventInfo)
        => eventInfo.DeclaringType?.Assembly.GetName().Name == "ProjectMER";

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
