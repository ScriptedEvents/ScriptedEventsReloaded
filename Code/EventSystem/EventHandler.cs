using System.Linq.Expressions;
using System.Reflection;
using LabApi.Events;
using LabApi.Events.Arguments.Interfaces;
using LabApi.Loader;
using PlayerStatsSystem;
using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.ScriptSystem.Structures;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;
using SER.Code.VariableSystem.Bases;
using DamageHandlerBase = PlayerStatsSystem.DamageHandlerBase;

namespace SER.Code.EventSystem;

public static class EventHandler
{
    private static readonly List<Action> UnsubscribeActions = [];
    private static readonly Dictionary<string, List<Action<EventArgs?, Variable[]>>> OnEventActions = [];
    private static readonly HashSet<string> DisabledEvents = [];
    public static List<EventInfo> AvailableEvents = [];
    public static readonly HashSet<string> RegisteredHandlers = [];
    public static readonly HashSet<string> BindedEvents = [];
    
    public static void Initialize()
    {
        Clear();
        AvailableEvents = typeof(PluginLoader).Assembly.GetTypes()
            .Where(t => t.FullName?.Equals($"LabApi.Events.Handlers.{t.Name}") is true)
            .Select(t => t.GetEvents(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public 
                                     | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).ToList())
            .Flatten().ToList();
    }
    
    public static void Clear()
    {
        RegisteredHandlers.Clear();
        OnEventActions.Clear();
        foreach (var unsubscribeAction in UnsubscribeActions)
        {
            unsubscribeAction();
        }
        UnsubscribeActions.Clear();
        DisabledEvents.Clear();
        BindedEvents.Clear();
    }

    public static Result DisableEvent(string evName)
    {
        DisabledEvents.Add(evName);
        return BindEvent(evName);
    }

    public static bool EnableEvent(string evName)
    {
        DisabledEvents.Remove(evName);
        return false;
    }
    
    public static Result AddEventHandler(string evName, ScriptName scriptName) 
    {
        if (RegisteredHandlers.Contains($"'{scriptName}' script"))
        {
            return true;
        }
        
        if (BindEvent(evName).HasErrored(out var error))
        {
            return error;
        }
        
        RegisteredHandlers.Add($"'{scriptName}' script");
        if (OnEventActions.TryGetValue(evName, out var actions))
        {
            actions.Add(RunScriptOnEvent(scriptName, evName));
            return true;
        }
        
        OnEventActions.Add(evName, [RunScriptOnEvent(scriptName, evName)]);
        return true;
    }
    
    public static Result AddEventHandler(string evName, Action<EventArgs?, Variable[]> action, string handlerId) 
    {
        if (RegisteredHandlers.Contains(handlerId))
        {
            return $"{handlerId}' is already registered as an event handler!";
        }
        
        if (BindEvent(evName).HasErrored(out var error))
        {
            return error;
        }
        
        RegisteredHandlers.Add(handlerId);
        if (OnEventActions.TryGetValue(evName, out var actions))
        {
            actions.Add(action);
        }
        else
        {
            OnEventActions.Add(evName, [action]);
        }
        
        return true;
    }

    private static Result BindEvent(string evName)
    {
        if (!BindedEvents.Add(evName))
        {
            // already binded
            return true;
        }

        EventInfo? matchingEventInfo = AvailableEvents.FirstOrDefault(e => e.Name == evName);
        if (matchingEventInfo is null)
        {
            return $"Event '{evName}' does not exist!"; 
        }
        
        var genericType = matchingEventInfo.EventHandlerType.GetGenericArguments().FirstOrDefault();
        if (genericType is not null)
        {
            BindArgumented(matchingEventInfo, genericType);
            return true;
        }
        
        BindNonArgumented(matchingEventInfo);
        return true;
    }

    private static Action<EventArgs?, Variable[]> RunScriptOnEvent(ScriptName scrName, string evName)
    {
        return (ev, variables) =>
        {
            Result rs = $"Failed to run script '{scrName}' connected to event '{evName}'";
            Log.Debug($"Running script '{scrName}' for event '{evName}'");

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

    private static void BindNonArgumented(EventInfo eventInfo)
    {
        var evName = eventInfo.Name;

        // Create delegate that captures evName
        LabEventHandler handler = () => OnNonArgumentedEvent(evName);

        // Subscribe
        eventInfo.GetAddMethod(false).Invoke(null!, [handler]);

        // Store unsubscribe action
        UnsubscribeActions.Add(() => eventInfo.GetRemoveMethod(false).Invoke(null!, [handler]));
    }

    private static void BindArgumented(EventInfo eventInfo, Type generic)
    {
        var evName = eventInfo.Name;

        // We'll build (T ev) => OnArgumentedEvent(evName, ev)
        var evParam = Expression.Parameter(generic, "ev");
        var nameConst = Expression.Constant(evName);
        var call = Expression.Call(
            typeof(EventHandler)
                .GetMethod(nameof(OnArgumentedEvent), BindingFlags.Static | BindingFlags.NonPublic)!
                .MakeGenericMethod(generic),
            nameConst,
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

    private static void OnNonArgumentedEvent(string evName)
    {
        Log.Debug($"[NonArg] Event '{evName}' triggered.");

        if (!OnEventActions.TryGetValue(evName, out var actions))
            return;

        foreach (var action in actions) action(null, []);
    }

    private static void OnArgumentedEvent<T>(string evName, T ev) where T : EventArgs
    {
        Log.Debug($"[Arg] Event '{evName}' triggered with {typeof(T).AccurateName}.");

        if (ev is ICancellableEvent cancellable && DisabledEvents.Contains(evName))
        {
            cancellable.IsAllowed = false;
            Log.Debug($"Event '{evName}' cancelled (disabled).");
            return;
        }

        var variables = GetVariablesFromEvent(ev);
        if (!OnEventActions.TryGetValue(evName, out var actions))
        {
            Log.Debug($"Event '{evName}' has no scripts connected.");
            return;
        }

        foreach (var action in actions) action(ev, variables);
    }
    
    public static Variable[] GetVariablesFromEvent(EventArgs ev)
    {
        List<(object, string, Type)> properties = (
            from prop in ev.GetType().GetProperties()
            where !Attribute.IsDefined(prop, typeof(ObsoleteAttribute))
            let value = prop.GetValue(ev)
            let type = prop.PropertyType
            select (value, prop.Name, type)
        ).ToList();

        return InternalGetVariablesFromProperties(properties);
    }
    
    public static List<string> GetMimicVariables(EventInfo ev)
    {
        if (ev.EventHandlerType.GetGenericArguments().FirstOrDefault() is not { } genericType)
        {
            return [];
        }

        List<(Type, string)> properties = (
            from prop in genericType.GetProperties()
            where !Attribute.IsDefined(prop, typeof(ObsoleteAttribute))
            let value = prop.PropertyType
            where value is not null
            select (value, prop.Name)
        ).ToList();
        
        return GetMimicVariablesForEventHelp(properties);
    }

    private static Variable[] InternalGetVariablesFromProperties(List<(object value, string name, Type type)> properties)
    {
        List<Variable> variables = [];
        foreach (var (value, name, _) in properties)
        {
            if (value is null) continue;
            variables.Add(Variable.Create(
                $"ev{name[0].ToString().ToUpper()}{name[1..]}", 
                Value.Parse(value, null))
            );
        }

        return variables.ToArray();
    }
    
    private static List<string> GetMimicVariablesForEventHelp(List<(Type type, string name)> properties)
    {
        List<string> variables = [];
        foreach (var (type, name) in properties)
        {
            if (type is null) continue;
            var typeOfValue = new SingleTypeOfValue(Value.GuessValueType(type));
            
            // because of stupid NW design decision, only StandardDamageHandler inherits from DamageHandlerBase
            if (typeOfValue.Is<ReferenceValue<DamageHandlerBase>>())
            {
                typeOfValue = new TypeOfValue<ReferenceValue<StandardDamageHandler>>();
            }
            
            variables.Add($"{Value.GetPrefixOfValue(typeOfValue)}ev{name} ({typeOfValue})");
        }

        return variables;
    }
}