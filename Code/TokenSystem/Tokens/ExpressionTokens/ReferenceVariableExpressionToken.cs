using LabApi.Features.Wrappers;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;

namespace SER.Code.TokenSystem.Tokens.ExpressionTokens;

public class ReferenceVariableExpressionToken : ExpressionToken
{
    private ReferenceVariableToken _refVarToken = null!;
    private string _property = null!;
    
    protected override IParseResult InternalParse(BaseToken[] tokens)
    {
        if (tokens.First() is not ReferenceVariableToken refVarToken)
        {
            return new Ignore();
        }
        
        _refVarToken = refVarToken;
        
        if (tokens.Length > 2)
        {
            return new Error(
                "When accessing properties of a reference variable, only 2 arguments are allowed: " +
                $"the variable and the property to access. You provided {tokens.Length}."
            );
        }

        _property = tokens.Last().GetBestTextRepresentation(null);
        return new Success();
    }

    public override TryGet<Value> Value()
    {
        if (_refVarToken.TryGetVariable().HasErrored(out var err, out var variable))
        {
            return err;
        }

        return GetProperty(variable, _property);
    }

    public override TypeOfValue PossibleValues => new UnknownTypeOfValue();

    public abstract class Info
    {
        public abstract Func<object, Value> Handler { get; }
        public abstract SingleTypeOfValue ReturnType { get; }
        public abstract string? Description { get; }
    }
    
    public class Info<TIn, TOut>(Func<TIn, TOut> handler, string? description) : Info
        where TOut : Value
    {
        public override Func<object, Value> Handler => input => handler((TIn)input);
        public override SingleTypeOfValue ReturnType => new(typeof(TOut));
        public override string? Description => description;
    }

    public static TypesOfValue GetTypesOfValue(Type type)
    {
        return new TypesOfValue(
            PropertyInfoMap[typeof(Item)]
                .Select(i => i.Value.ReturnType)
                .ToArray()
        );
    }
    
    public static TryGet<Value> GetProperty(object obj, string propertyName)
    {
        var objType = obj.GetType();
        while (objType != null)
        {
            if (PropertyInfoMap.TryGetValue(objType, out var props) && props.TryGetValue(propertyName, out var info))
            {
                return info.Handler(obj);
            }
            
            objType = objType.BaseType;
        }
        
        return $"Type {obj.GetType().AccurateName} has no property '{propertyName}' or this type is not supported.";
    }

    public static readonly Dictionary<Type, Dictionary<string, Info>> PropertyInfoMap = new()
    {
        [typeof(Item)] = new()
        {
            ["type"] = new Info<Item, StaticTextValue>(item => item.Type.ToString().ToStaticTextValue(), null),
            ["category"] = new Info<Item, StaticTextValue>(item => item.Category.ToString().ToStaticTextValue(), null),
            ["owner"] = new Info<Item, PlayerValue>(item => new PlayerValue(item.CurrentOwner), null),
            ["isEquipped"] = new Info<Item, BoolValue>(item => new BoolValue(item.IsEquipped), null)
        }
    };
}

