using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.ArgumentSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.FileSystem.Structures;
using SER.Code.Helpers;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.MethodSystem.Methods.CustomRoleMethods.Structures;
using SER.Code.ScriptSystem;
using SER.Code.ScriptSystem.Structures;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Bases;
using SER.Code.VariableSystem.Variables;
using UnityEngine;

namespace SER.Code.ArgumentSystem;

public class ProvidedArguments(Method method)
{
    private Dictionary<(string name, Type type), List<OldDynamicTryGet>> ArgumentValues { get; } = [];

    public string GetEvent(string argName)
    {
        return GetValue<string, EventArgument>(argName);
    }
    
    public CRole GetCustomRole(string argName)
    {
        return GetValue<CRole, CustomRoleArgument>(argName);
    }
    
    public CallbackArgument.Callback GetCallback(string argName)
    {
        return GetValue<CallbackArgument.Callback, CallbackArgument>(argName);
    }
    
    public Generator[] GetGenerators(string argName)
    {
        return GetValue<Generator[], GeneratorsArgument>(argName);
    }
    
    public T GetVariable<T>(string argName) where T : Variable
    {
        return GetValue<T, VariableArgument<T>>(argName);
    }
    
    public Database GetDatabase(string argName)
    {
        return GetValue<Database, DatabaseArgument>(argName);
    }
    
    public Script GetCreatedScript(string argName)
    {
        return GetValue<Script, CreatedScriptArgument>(argName);
    }
    
    public ScriptName GetScriptName(string argName)
    {
        return GetValue<ScriptName, ScriptNameArgument>(argName);
    }
    
    public bool GetIsValidReference(string argName)
    {
        return GetValue<bool, IsValidReferenceArgument>(argName);
    }
    
    public T GetToken<T>(string argName) where T : BaseToken
    {
        return GetValue<T, TokenArgument<T>>(argName);
    }
    
    public T GetValue<T>(string argName) where T : Value
    {
        return GetValue<T, ValueArgument<T>>(argName);
    }
    
    public Value GetAnyValue(string argName)
    {
        return GetValue<Value, AnyValueArgument>(argName);
    }
    
    public CollectionValue GetCollection(string argName)
    {
        return GetValue<CollectionValue, CollectionArgument>(argName);
    }
    
    public Room GetRoom(string argName)
    {
        return GetValue<Room, RoomArgument>(argName);
    }
    
    public Elevator[] GetElevators(string argName)
    {
        return GetValue<Elevator[], ElevatorsArgument>(argName);
    }
    
    public LiteralVariable GetLiteralVariable(string argName)
    {
        return GetValue<LiteralVariable, LiteralVariableArgument>(argName);
    }
    
    public Item[] GetItems(string argName)
    {
        return GetValue<Item[], ItemsArgument>(argName);
    }

    public VariableToken GetVariableName(string argName)
    {
        return GetValue<VariableToken, VariableNameArgument>(argName);
    }

    public T GetVariableName<T>(string argName)
        where T : VariableToken
    {
        return GetValue<T, VariableNameArgument<T>>(argName);
    }
    
    public Variable GetVariable(string argName)
    {
        return GetValue<Variable, VariableArgument>(argName);
    }
    
    public Script GetRunningScript(string argName)
    {
        return GetValue<Script, RunningScriptArgument>(argName);
    }

    public Color? GetNullableColor(string argName)
    {
        return GetValueNullableStruct<Color, ColorArgument>(argName);
    }

    public Color GetColor(string argName)
    {
        return GetValue<Color, ColorArgument>(argName);
    }
    
    public Room[] GetRooms(string argName)
    {
        return GetValue<Room[], RoomsArgument>(argName);
    }
    
    public bool GetBool(string argName)
    {
        return GetValue<bool, BoolArgument>(argName);
    }

    public bool? GetNullableBool(string argName)
    {
        return GetValueNullableStruct<bool, BoolArgument>(argName);
    }

    public T GetLooseReference<T>(string argName)
    {
        return (T)GetValue<object, LooseReferenceArgument>(argName);
    }
    
    public T GetReference<T>(string argName)
    {
        return GetValue<T, ReferenceArgument<T>>(argName);
    }
    
    public Door[] GetDoors(string argName)
    {
        return GetValue<Door[], DoorsArgument>(argName);
    }
    
    public Door GetDoor(string argName)
    {
        return GetValue<Door, DoorArgument>(argName);
    }

    public Gate GetGate(string argName)
    {
        return GetValue<Gate, GateArgument>(argName);
    }

    public TimeSpan? GetNullableDuration(string argName)
    {
        return GetValueNullableStruct<TimeSpan, DurationArgument>(argName);
    }

    public TimeSpan GetDuration(string argName)
    {
        return GetValue<TimeSpan, DurationArgument>(argName);
    }

    public string GetText(string argName)
    {
        return GetValue<string, TextArgument>(argName);
    }

    public Func<Player[]> GetPlayersFunc(string argName)
    {
        var getter = GetGetter<Player[], PlayersArgument>(argName);
        return () => getter.Invoke().Value ?? [];
    }
    
    public Player[] GetPlayers(string argName)
    {
        return GetValue<Player[], PlayersArgument>(argName);
    }

    public Player GetPlayer(string argName)
    {
        return GetValue<Player, PlayerArgument>(argName);
    }

    public float GetFloat(string argName)
    {
        return GetValue<float, FloatArgument>(argName);
    }

    public float? GetNullableFloat(string argName)
    {
        return GetValueNullableStruct<float, FloatArgument>(argName);
    }

    public int GetInt(string argName)
    {
        return GetValue<int, IntArgument>(argName);
    }

    public int? GetNullableInt(string argName)
    {
        return GetValueNullableStruct<int, IntArgument>(argName);
    }

    public TEnum? GetNullableEnum<TEnum>(string argName) where TEnum : struct, Enum
    {
        return GetValueNullableStruct<TEnum, EnumArgument<TEnum>>(argName);
    }
    
    public TEnum GetEnum<TEnum>(string argName) where TEnum : struct, Enum
    {
        return GetValue<TEnum, EnumArgument<TEnum>>(argName);
    }
    
    /// <remarks>
    /// Return value is always lowercase!
    /// </remarks>
    public string GetOption(string argName)
    {
        return GetValue<string, OptionsArgument>(argName).ToLowerInvariant();
    }

    public Type GetEffectType(string argName)
    {
        return GetValue<Type, EffectTypeArgument>(argName);
    }

    public RespawnWave? GetWave(string argName)
    {
        return GetValue<RespawnWave?, WaveArgument>(argName);
    }

    public CollectionVariable GetCollectionVariable(string argName)
    {
        return GetValue<CollectionVariable, CollectionVariableArgument>(argName);
    }

    /// <summary>
    /// Retrieves a list of remaining arguments based on the specified argument name.
    /// The method resolves provided arguments into a typed list of values.
    /// </summary>
    public TValue[] GetRemainingArguments<TValue, TArg>(string argName) 
        where TArg : Argument
    {
        return GetValues<TValue, TArg>(argName).Select(x => x.value).ToArray();
    }

    public TValue GetValue<TValue, TArg>(string argName) 
        where TArg : Argument
    {
        return GetValues<TValue, TArg>(argName)[0].value;
    }
    
    public OldDynamicTryGet<TValue> GetGetter<TValue, TArg>(string argName) 
        where TArg : Argument
    {
        return GetValues<TValue, TArg>(argName)[0].getter;
    }
    
    public TValue? GetValueNullableStruct<TValue, TArg>(string argName) 
        where TArg : Argument
        where TValue : struct
    {
        var evaluator = GetValueInternal<TValue?, TArg>(argName).First();
        
        if (evaluator.Result.HasErrored(out var error))
        {
            throw new CustomScriptRuntimeError(
                $"Fetching argument '{argName}' for method '{method.Name}' failed.".AsOldError() 
                + error
            );
        }
        
        return evaluator switch
        {
            OldDynamicTryGet<TValue> strict => strict.Invoke().Value,
            OldDynamicTryGet<TValue?> nullable => nullable.Invoke().Value,
            _ => throw new AndrzejFuckedUpException(
                $"Argument '{argName}' evaluator type mismatch. " +
                $"Got {evaluator.GetType().AccurateName}, expected {typeof(TValue).AccurateName} or {typeof(TValue?).AccurateName}.")
        };
    }

    private List<(TValue value, OldDynamicTryGet<TValue> getter)> GetValues<TValue, TArg>(string argName)
        where TArg : Argument
    {
        OldResult mainErr = $"Fetching argument '{argName}' for method '{method.Name}' failed.";

        var evaluators = GetValueInternal<TValue, TArg>(argName);

        List<(TValue, OldDynamicTryGet<TValue>)> resultList = [];
        foreach (var evaluator in evaluators)
        {
            if (evaluator is not OldDynamicTryGet<TValue> argEvalRes)
            {
                throw new AndrzejFuckedUpException(
                    mainErr +
                    $"Argument value is not of type {typeof(TValue).Name}, evaluator: {evaluator.GetType().AccurateName}."
                );
            }

            if (argEvalRes.Invoke().HasErrored(out var err, out var value))
            {
                throw new CustomScriptRuntimeError(mainErr + err);
            }

            resultList.Add((value, argEvalRes));
        }
        
        return resultList;
    }

    private List<OldDynamicTryGet> GetValueInternal<TValue, TArg>(string argName) 
        where TArg : Argument
    {
        if (ArgumentValues.TryGetValue((argName, typeof(TArg)), out var value))
        {
            return value;
        }

        var foundArg = method.ExpectedArguments.FirstOrDefault(arg => arg.Name == argName);
        if (foundArg is null)
        {
            throw new AndrzejFuckedUpException($"There is no argument registered of type '{nameof(TArg)}' and name '{argName}'.");
        }

        if (foundArg.DefaultValue is null)
        {
            throw new ScriptRuntimeError(method, $"Method is missing a required argument '{argName}'.");
        }

        return foundArg.DefaultValue.Value switch
        {
            TValue argValue => [
                new OldDynamicTryGet<TValue>(argValue)
            ],
            
            IEnumerable<TValue> listValue => listValue
                .Select(OldDynamicTryGet (v) => new OldDynamicTryGet<TValue>(v))
                .ToList(),
            
            null => [
                new OldDynamicTryGet<TValue>((TValue)(object)null!)
            ], // magik
            
            _ => throw new AndrzejFuckedUpException(
                $"Argument {argName} for method {method.Name} has its default value set to type " +
                $"{foundArg.DefaultValue?.Value.GetType().AccurateName ?? "null"}, expected of type {typeof(TValue).Name} or a list of " +
                $"{typeof(TValue).Name}s."
            )
        };
    }

    public void Add(ArgumentValueInfo valueInfo)
    {
        Log.Debug($"adding {valueInfo.Name} for method {method.Name} ({method.GetHashCode()})");
        if (!valueInfo.IsPartOfCollection)
        {
            ArgumentValues.Add((valueInfo.Name, valueInfo.ArgumentType), [valueInfo.Evaluator]);
            return;
        }
        
        ArgumentValues.AddOrInitListWithKey((valueInfo.Name, valueInfo.ArgumentType), valueInfo.Evaluator);
    }
}