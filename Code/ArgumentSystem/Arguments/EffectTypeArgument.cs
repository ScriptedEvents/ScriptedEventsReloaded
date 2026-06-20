using CustomPlayerEffects;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ArgumentSystem.Arguments;

public class EffectTypeArgument(string name) : Argument(name)
{
    public static readonly Type[] EffectTypes = typeof(StatusEffectBase).Assembly.GetTypes()
        .Where(t =>
            t.IsSubclassOf(typeof(StatusEffectBase)) &&
            !t.IsAbstract &&
            !typeof(IHolidayEffect).IsAssignableFrom(t)
        )
        .ToArray();

    public static readonly Dictionary<string, Type> EffectNames = EffectTypes
        .ToDictionary(t => t.Name, t => t, StringComparer.OrdinalIgnoreCase);

    public override string InputDescription =>
        "One of the following effects:\n"
        + EffectNames.Keys.Select(n => $"> {n}").JoinStrings("\n");

    [UsedImplicitly]
    public OldDynamicTryGet<Type> GetConvertSolution(BaseToken token)
    {
        if (token.BestTextRepr().IsStatic(out var name, out var func))
        {
            return InternalConvert(name);
        }

        return new(() => InternalConvert(func()));
    }

    private static OldTryGet<Type> InternalConvert(string name)
    {
        if (EffectNames.TryGetValue(name, out var type))
        {
            return type;
        }

        return "Value is not a valid effect name.";
    }
}