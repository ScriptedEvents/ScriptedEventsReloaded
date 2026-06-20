using System.Linq.Expressions;
using System.Reflection;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.ArgumentSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ArgumentSystem;

public class MethodArgumentDispatcher(Method method)
{
    private class ConverterInfo
    {
        private Func<Argument, BaseToken, OldDynamicTryGet> Delegate { get; }

        public ConverterInfo(MethodInfo methodInfo)
        {
            var instanceParam = Expression.Parameter(typeof(Argument), "instance");
            var tokenParam = Expression.Parameter(typeof(BaseToken), "token");

            var call = Expression.Call(
                Expression.Convert(instanceParam, methodInfo.DeclaringType!),
                methodInfo,
                tokenParam
            );

            var lambda = Expression.Lambda<Func<Argument, BaseToken, OldDynamicTryGet>>(call, instanceParam, tokenParam);
            Delegate = lambda.Compile();
        }
        
        public OldDynamicTryGet Invoke(BaseToken token, Argument arg)
        {
            try
            {
                return Delegate(arg, token);
            }
            catch (Exception ex)
            {
                return OldDynamicTryGet.Error($"This error is not an expected one, report it to the developers. {ex.Message} {ex.StackTrace}");
            }
        }
    }
    
    private static readonly Dictionary<Type, ConverterInfo> ConverterCache = new();
    
    private static ConverterInfo GetConverterInfo(Type argType)
    {
        if (ConverterCache.TryGetValue(argType, out var cachedInfo))
        {
            return cachedInfo;
        }
        
        var instanceMethod = argType.GetMethod("GetConvertSolution", 
            BindingFlags.Public | BindingFlags.Instance,
            null, [typeof(BaseToken)], null);
            
        if (instanceMethod != null)
        {
            return ConverterCache[argType] = new ConverterInfo(instanceMethod);
        }
        
        throw new AndrzejFuckedUpException($"No suitable GetConvertSolution method found for {argType.AccurateName}.");
    }

    public OldTryGet<ArgumentValueInfo?> TryGetValueInfo(BaseToken token, int index)
    {
        OldResult rs = $"Argument {index + 1} '{token.RawRep}' for method {method.Name} is invalid.";

        Argument arg;
        if (index >= method.ExpectedArguments.Length)
        {
            if (method.ExpectedArguments.LastOrDefault() is not { ConsumesRemainingValues: true } lastArg)
            {
                return rs + $"Method does not expect more than {method.ExpectedArguments.Length} arguments.";
            }
            
            arg = lastArg;
        }
        else
        {
            arg = method.ExpectedArguments[index];
        }

        if (token.RawRep == "_")
        {
            if (arg.DefaultValue is null)
            {
                return rs + "This argument is required, you cannot skip providing it by using the floor character.";
            }

            return OldTryGet<ArgumentValueInfo?>.Success(null);
        }
        
        arg.Script = method.Script;
        var argType = arg.GetType();
        
        var evaluator = GetConverterInfo(argType).Invoke(token, arg);
        if (!evaluator.Static)
        {
            return new ArgumentValueInfo
            {
                Evaluator = evaluator,
                ArgumentType = argType,
                Name = arg.Name,
                IsPartOfCollection = arg.ConsumesRemainingValues
            };
        }
        
        if (evaluator.Result.HasErrored(out var error))
        {
            Log.D($"error from evaluator: {error}");
            Log.D($"default error: {rs}");
            return rs + error;
        }

        return new ArgumentValueInfo
        {
            Evaluator = evaluator,
            ArgumentType = argType,
            Name = arg.Name,
            IsPartOfCollection = arg.ConsumesRemainingValues
        };
    }
}