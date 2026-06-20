using SER.Code.Extensions;
using SER.Code.ResultSystem;
using SER.Code.ValueSystem;
using ValueType = SER.Code.ValueSystem.ValueType;

namespace SER.Code.PropertySystem;

public interface IValueWithProperties
{
    public abstract class PropInfo
    {
        public abstract TryGet<Value> GetValue(object obj);
        public virtual Result SetValue(object obj, Value value) => "This property is read-only.".AsError();
        public abstract ValueMetadata ReturnType { get; }
        public abstract string? Description { get; }
        public virtual bool IsReflected => false;
        public virtual bool IsSettable => false;
    }
    
    public class PropInfo<TIn>(ValueType valueType, Func<TIn, Value> handler, string? description) : PropInfo
    {
        public Func<TIn, Value> Func => handler;
        protected Func<object, object>? Translator => null;

        public override TryGet<Value> GetValue(object obj)
        {
            if (Translator is not null) obj = Translator(obj);
            if (obj is not TIn inObj) return $"Provided value is not of type {typeof(TIn).AccurateName}".AsError();
            try
            {
                return handler(inObj);
            }
            catch (Exception e)
            {
                return $"Failed to get property: {e.Message}".AsError();
            }
        }

        public override ValueMetadata ReturnType => ValueMetadata.Basic(valueType);
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