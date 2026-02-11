using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem;

namespace SER.Code.MethodSystem.Methods.VariableMethods;

[UsedImplicitly]
public class GlobalVariablesMethod : ReturningMethod<CollectionValue>
{
    public override string Description => "Returns a collection containing all the global variable names";

    public override Argument[] ExpectedArguments { get; } = [];
    
    public override void Execute()
    {
        ReturnValue = new CollectionValue(VariableIndex.GlobalVariables
            .Select(variable => variable.Prefix + variable.Name));
    }
}