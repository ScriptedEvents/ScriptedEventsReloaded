using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.GeneralVariableMethods;

[UsedImplicitly]
public class VarExistsMethod : ReturningMethod<BoolValue>
{
    public override string Description => "Returns a bool value indicating if the provided variable exists.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TokenArgument<VariableToken>("variable")
    ];
    
    public override void Execute()
    {
        var token = Args.GetToken<VariableToken>("variable");
        ReturnValue = token.TryGetVariable().WasSuccessful();
    }
}