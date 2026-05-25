using SER.Code.ContextSystem.Structures;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem.Other;

namespace SER.Code.ValueSystem.PropertySystem;

public class PropertyAccess(BaseToken initialToken, IValueToken root)
{
    private readonly List<string> _propertyNames = [];

    public TypeOfValue PossibleValues { get; private set; } = root.PossibleValues;

    public string ExprRepr { get; private set; } = initialToken.RawRep;

    public int PropertyCount => _propertyNames.Count;

    public TryAddTokenRes TryAddToken(BaseToken token)
    {
        if (token is SymbolToken { IsArrow: true })
        {
            ExprRepr += $" {token.RawRep}";
            return TryAddTokenRes.Continue();
        }

        // type verification
        if (PossibleValues.AreKnown(out var types))
        {
            foreach (var type in types)
            {
                if (type == typeof(ReferenceValue))
                {
                    ExprRepr += $" {token.RawRep}";
                    PossibleValues = new UnknownTypeOfValue();
                    goto found;
                }

                var props = Value.GetPropertiesOfValue(type);
                if (props == null && type == typeof(LiteralValue))
                {
                    foreach (var subType in LiteralValue.Subclasses)
                    {
                        var subProps = Value.GetPropertiesOfValue(subType);
                        if (subProps?.TryGetValue(token.RawRep, out var subProp) is true)
                        {
                            ExprRepr += $" {token.RawRep}";
                            PossibleValues = subProp.ReturnType;
                            goto found;
                        }
                    }

                    ExprRepr += $" {token.RawRep}";
                    PossibleValues = new UnknownTypeOfValue();
                    goto found;
                }
                if (props is IValueWithProperties.IDynamicPropertyDictionary dynamicDict)
                {
                    if (dynamicDict.TryGetValue(token.RawRep, out var dynamicProp))
                    {
                        ExprRepr += $" {token.RawRep}";
                        PossibleValues = dynamicProp.ReturnType;
                        goto found;
                    }
                }
                else if (props?.TryGetValue(token.RawRep, out var property) is true)
                {
                    ExprRepr += $" {token.RawRep}";
                    PossibleValues = property.ReturnType;
                    goto found;
                }
            }

            return TryAddTokenRes.Error($"'{token.RawRep}' is not a valid property of {PossibleValues}.");
        }

        found:
        _propertyNames.Add(token.RawRep);
        return TryAddTokenRes.Continue();
    }

    public TryGet<Value> ResolveValue()
    {
        var lastPropRes = ResolveLastProp();
        if (lastPropRes.HasErrored(out var error, out var lastProp)) return error;

        return lastProp.propInfo.GetValue(lastProp.target);
    }

    public TryGet<(object target, IValueWithProperties.PropInfo propInfo)> ResolveLastProp()
    {
        if (root.Value().HasErrored(out var rootError, out var rootValue))
        {
            return $"Failed to get value from '{ExprRepr}'".AsError() + rootError.AsError();
        }

        if (_propertyNames.Count == 0)
        {
            return "No properties specified.";
        }

        Value current = rootValue;
        for (int i = 0; i < _propertyNames.Count; i++)
        {
            var prop = _propertyNames[i];
            if (current is not IValueWithProperties propVal)
            {
                return $"{current.FriendlyName} does not have any properties.";
            }

            IValueWithProperties.PropInfo? propInfo;
            if (propVal.Properties is IValueWithProperties.IDynamicPropertyDictionary dynamicDict)
            {
                if (!dynamicDict.TryGetValue(prop, out propInfo))
                {
                    return $"{current.FriendlyName} does not have property '{prop}'.";
                }
            }
            else if (!propVal.Properties.TryGetValue(prop, out propInfo))
            {
                return $"{current.FriendlyName} does not have property '{prop}'.";
            }

            if (i == _propertyNames.Count - 1)
            {
                return (current, propInfo);
            }

            if (propInfo.GetValue(current).HasErrored(out var fetchError, out var fetchedValue))
            {
                return fetchError;
            }

            current = fetchedValue;
        }

        return "Should not reach here";
    }
}
