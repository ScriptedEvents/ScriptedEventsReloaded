using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.ArgumentSystem.Structures;
using SER.Code.FileSystem.Structures;
using SER.Code.Helpers;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.MethodSystem.BaseMethods;
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
    private Dictionary<(string name, Type type), List<DynamicTryGet>> ArgumentValues { get; } = [];

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
    
    public LiteralVariableToken GetLiteralVariable(string argName)
    {
        return GetValue<LiteralVariableToken, LiteralVariableArgument>(argName);
    }
    
    public Item[] GetItems(string argName)
    {
        return GetValue<Item[], ItemsArgument>(argName);
    }
    
    public PlayerVariableToken GetPlayerVariableName(string argName)
    {
        return GetValue<PlayerVariableToken, PlayerVariableNameArgument>(argName);
    }
    
    public Variable GetVariable(string argName)
    {
        return GetValue<Variable, VariableArgument>(argName);
    }
    
    public Script GetRunningScript(string argName)
    {
        return GetValue<Script, RunningScriptArgument>(argName);
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

    public Func<bool> GetBoolFunc(string argName)
    {
        var evaluator = GetEvaluators<bool, BoolArgument>(argName).First();
        return () => evaluator.Invoke().Value;
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

    public TimeSpan GetDuration(string argName)
    {
        return GetValue<TimeSpan, DurationArgument>(argName);
    }

    public string GetText(string argName)
    {
        return GetValue<string, TextArgument>(argName);
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

    public int GetInt(string argName)
    {
        return GetValue<int, IntArgument>(argName);
    }

    public TEnum GetEnum<TEnum>(string argName) where TEnum : struct, Enum
    {
        var obj = GetValue<object, EnumArgument<TEnum>>(argName);
        if (obj is not TEnum value)
            throw new AndrzejFuckedUpException($"Enum got {obj.GetType().AccurateName}, not {typeof(TEnum).AccurateName}");

        return value;
    }
    
    /// <remarks>
    /// Return value is always lowercase!
    /// </remarks>
    public string GetOption(string argName)
    {
        return GetValue<string, OptionsArgument>(argName).ToLower();
    }

    public CollectionVariable GetCollectionVariable(string argName)
    {
        return GetValue<CollectionVariable, CollectionVariableArgument>(argName);
    }

    /// <summary>
    /// Retrieves a list of remaining arguments based on the specified argument name.
    /// The method resolves provided arguments into a typed list of values.
    /// </summary>
    public TValue[] GetRemainingArguments<TValue, TArg>(string argName) where TArg : Argument
    {
        return GetEvaluators<TValue, TArg>(argName).Select(dtg => dtg.Invoke().Value!).ToArray();
    }

    public TValue GetValue<TValue, TArg>(string argName) where TArg : Argument
    {
        return GetEvaluators<TValue, TArg>(argName).First().Invoke().Value!;
    }

    private List<DynamicTryGet<TValue>> GetEvaluators<TValue, TArg>(string argName)
    {
        Result mainErr = 
            $"Fetching argument '{argName}' for method '{method.Name}' failed.";

        var evaluators = GetValueInternal<TValue, TArg>(argName);

        List<DynamicTryGet<TValue>> resultList = [];
        foreach (var evaluator in evaluators)
        {
            if (evaluator.Result.HasErrored(out var error))
            {
                throw new CustomScriptRuntimeError(mainErr + error);
            }

            if (evaluator is not DynamicTryGet<TValue> argEvalRes)
                throw new AndrzejFuckedUpException(
                    mainErr + $"Argument value is not of type {typeof(TValue).Name}, evaluator: {evaluator.GetType().AccurateName}.");
            
            resultList.Add(argEvalRes);
        }
        
        return resultList;
    }

    private List<DynamicTryGet> GetValueInternal<TValue, TArg>(string argName)
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
                new DynamicTryGet<TValue>(argValue)
            ],
            
            IEnumerable<TValue> listValue => listValue
                .Select(DynamicTryGet (v) => new DynamicTryGet<TValue>(v))
                .ToList(),
            
            null => [
                new DynamicTryGet<TValue>((TValue)(object)null!)
            ], // magik
            
            _ => throw new AndrzejFuckedUpException(
                $"Argument {argName} for method {method.Name} has its default value set to type " +
                $"{foundArg.DefaultValue?.GetType().AccurateName ?? "null"}, expected of type {typeof(TValue).Name} or a list of " +
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