using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.MethodSystem.Methods.PlayerVariableMethods;

[UsedImplicitly]
public class OverlappingMethod : ReturningMethod<BoolValue>
{
    public override string Description => "Checks if all player variables have the same players.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new VariableArgument<PlayerVariable>("player variables")
        {
            ConsumesRemainingValues = true
        }
    ];
    
    public override void Execute()
    {
        var variables = Args
            .GetRemainingArguments<PlayerVariable, VariableArgument<PlayerVariable>>("player variables");

        if (variables.Length <= 1)
        {
            ReturnValue = true;
            return;
        };
        
        int startHash = variables[0].Value.GetHashCode();
        ReturnValue = new BoolValue(variables.Skip(1).All(v => v.Value.GetHashCode() == startHash));
    }
}