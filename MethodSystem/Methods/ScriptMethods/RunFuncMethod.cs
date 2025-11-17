using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.FlagSystem;
using SER.FlagSystem.Flags;
using SER.Helpers.Exceptions;
using SER.Helpers.Extensions;
using SER.MethodSystem.BaseMethods;
using SER.MethodSystem.MethodDescriptors;
using SER.ValueSystem;
using SER.VariableSystem.Bases;

namespace SER.MethodSystem.Methods.ScriptMethods;

public class RunFuncMethod : SynchronousMethod, ICanError
{
    public override string Description => "Runs a function script with arguments.";

    public string[] ErrorReasons =>
    [
        "Provided script is not a function",
        "Provided arguments are incompatible with the function"
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new CreatedScriptArgument("script to run"),
        new AnyValueArgument("values to pass")
        {
            ConsumesRemainingValues = true,
            DefaultValue = new(new List<Value>(), "none"),
            Description = "The values that will be passed to the function. " +
                          "These values must be compatible with the variables the function is expecting."
        }
    ];

    public override void Execute()
    {
        var scriptToRun = Args.GetCreatedScript("script to run");
        var valuesToPass = Args.GetRemainingArguments<Value, AnyValueArgument>("values to pass");

        if (ScriptFlagHandler.GetScriptFlags(scriptToRun.Name).FirstOrDefault(f => f.GetType() == typeof(FunctionFlag))
            is not FunctionFlag functionFlag)
        {
            throw new ScriptRuntimeError($"Script '{scriptToRun.Name}' is not a function.");
        }

        if (valuesToPass.Length != functionFlag.ExpectedVarTokens.Count)
        {
            throw new ScriptRuntimeError(
                $"Function expects {functionFlag.ExpectedVarTokens.Count} arguments, but {valuesToPass.Length} were provided."
            );
        }

        var zippedTuples = valuesToPass
            .Zip(functionFlag.ExpectedVarTokens, (v, t) => (value: v, varToken: t))
            .ToArray();

        for (var i = 0; i < zippedTuples.Length; i++)
        {
            if (zippedTuples[i] is not { varToken: { } varToken, value: { } value })
            {
                throw new AndrzejFuckedUpException();
            }
            
            if (!varToken.ValueType.IsInstanceOfType(value))
            {
                throw new ScriptRuntimeError(
                    $"Function '{scriptToRun.Name}' expects argument {i + 1} to be of type " +
                    $"{varToken.ValueType.FriendlyTypeName()}, but {value.GetType().FriendlyTypeName()} " +
                    $"was provided."
                );
            }
            
            var variable = Variable.CreateVariable(varToken.Name, value);
            if (variable.GetType() != varToken.VariableType)
            {
                throw new AndrzejFuckedUpException();
            }
            
            scriptToRun.AddVariable(variable);
        }
        
        scriptToRun.Run();
    }
}