using System;
using System.Reflection;
using SER.Code.Helpers;
using Log = Exiled.API.Features.Log;

namespace PluginSCPSL.Library.Utility;

public class Registerable<TSelf> where TSelf : Registerable<TSelf>
{
    public static RegisterableList<TSelf> Registered { get; } = new(OnAdd, OnRemove, () => {});

    public virtual bool IsDebug => false;
    public virtual bool ShouldRegister => true;
    private Assembly _assembly = null;

    private static void OnAdd(TSelf item)
    {
        item._assembly ??= typeof(TSelf).Assembly;
        item.OnRegistered();
    }

    private static void OnRemove(TSelf item)
    {
        item.OnUnregistered();
    }
    
    protected virtual void OnRegistered() { }
    protected virtual void OnUnregistered() { }
    
    public static void RegisterAll(Assembly assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();
        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract) continue;
            if (typeof(TSelf) == type)
            {
                Log.Info($"Skipping {type.Name}, because it's the original instance.");
                continue;
            }
            if (!type.IsSubclassOf(typeof(TSelf))) continue;
            if (type.GetConstructor([]) == null)
            {
                Log.Error($"Type {type.Name} could not be registered, please add default constructor");
                continue;
            }
            TSelf instance;
            try
            {
                instance = (TSelf)Activator.CreateInstance(type);
            }
            catch (Exception e)
            {
                Log.Error($"Instance registration error. Type: {type}\nMessage: {e}");
                continue;
            }
            instance._assembly = assembly;

            if (instance.ShouldRegister && instance.ShouldAddInstance())
            {
                if (instance.IsDebug)
                {
                    Log.Info($"Adding {type.Name} as {typeof(TSelf).Name}");
                }
                Registered.Add(instance);
            }
        }
    }

    public virtual TSelf CloneMyself()
    {
        if (this.GetType().GetConstructor([]) == null)
        {
            Log.Error($"{this.GetType().Name} does not have default constructor");
            return null;
        }
        return (TSelf)Activator.CreateInstance(this.GetType());
    }

    public virtual bool ShouldAddInstance()
    {
        return true;
    }

    public static void Register(TSelf instance)
    {
        Registered.Add(instance);
    }
    
    public static void Unregister(TSelf instance)
    {
        Registered.Remove(instance);
    }
    
    public static void UnregisterAll(Assembly assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();
        foreach (var i in Registered)
        {
            i.OnUnregistered();
        }
        Registered.RemoveAll(r => r._assembly == assembly);
    }
}