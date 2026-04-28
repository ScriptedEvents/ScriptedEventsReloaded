using LabApi.Features.Wrappers;
using Respawning;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ArgumentSystem.Arguments;

public class WaveTypeArgument(string name) : Argument(name)
{
    public static readonly Type[] WaveTypes = typeof(RespawnWave).Assembly.GetTypes()
        .Where(t => 
            t.IsSubclassOf(typeof(RespawnWave)) && 
            !t.IsAbstract
        )
        .ToArray();

    public override string InputDescription => 
        "One of the following wave types:\n" 
        + WaveTypes.Select(t => $"> {t.Name.LowerFirst()}").JoinStrings("\n"); 
    
    [UsedImplicitly]
    public DynamicTryGet<Type> GetConvertSolution(BaseToken token)
    {
        if (token.BestTextRepr().IsStatic(out var name, out var func))
        {
            return InternalConvert(name);
        }
        
        return new(() => InternalConvert(func()));
    }

    private static TryGet<Type> InternalConvert(string name)
    {
        if (WaveTypes.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)) is {} type)
        {
            return type;
        }
        
        return "Value is not a valid wave type.";
    }

    public static RespawnWave? GetWave(Type type)
    {
        foreach (var waveBase in WaveManager.Waves)
        {
            var wave = RespawnWaves.Get(waveBase);
            if (wave?.GetType() == type)
            {
                return wave;
            }
        }

        return null;
    }
}