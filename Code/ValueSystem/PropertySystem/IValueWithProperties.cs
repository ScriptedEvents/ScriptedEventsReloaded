using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ValueSystem.Other;

namespace SER.Code.ValueSystem.PropertySystem;

public interface IValueWithProperties
{
    public abstract class PropInfo
    {
        public abstract TryGet<Value> GetValue(object obj);
        public virtual Result SetValue(object obj, Value value) => "This property is read-only.";
        public abstract SingleTypeOfValue ReturnType { get; }
        public virtual TypeOfValue PossibleReturnTypes => ReturnType;
        public abstract string? Description { get; }
        public virtual bool IsReflected => false;
        public virtual bool IsSettable => false;
    }
    
    public abstract class PropInfo<T> : PropInfo
    {
        public abstract Func<T, Value> Func { get; }
    }

    public class PropInfo<TIn, TOut>(Func<TIn, TOut> handler, string? description) : PropInfo<TIn> 
        where TOut : Value
    {
        public override Func<TIn, Value> Func => handler;
        protected virtual Func<object, object>? Translator => null;

        public override TryGet<Value> GetValue(object obj)
        {
            if (Translator is not null) obj = Translator(obj);
            if (obj is not TIn inObj) return $"Provided value is not of type {typeof(TIn).AccurateName}";
            try
            {
                return handler(inObj);
            }
            catch (Exception e)
            {
                return $"Failed to get property: {e.Message}";
            }
        }

        public override SingleTypeOfValue ReturnType => new(typeof(TOut));
        public override string? Description => description;
    }
    
    public Dictionary<string, PropInfo> Properties { get; }

    public interface IDynamicPropertyDictionary : IDictionary<string, PropInfo>
    {
    }
}

/// <summary>
/// Marks that a value will change available properties based on its current state.
/// Used in reference value as properties depend on the current c# object.
/// </summary>
public interface IValueWithDynamicProperties : IValueWithProperties
{
}
