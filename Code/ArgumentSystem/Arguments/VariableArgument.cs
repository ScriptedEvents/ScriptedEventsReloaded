using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.ArgumentSystem.Arguments;

/// <summary>
/// Represents any Variable argument used in a method.
/// </summary>
public class VariableArgument(string name) : Argument(name)
{
    public override string InputDescription => "Any existing variable e.g. $name or @players";

    [UsedImplicitly]
    public DynamicTryGet<Variable> GetConvertSolution(BaseToken token)
    {
        if (token is not VariableToken variableToken)
        {
            return $"Value '{token.RawRep}' is not a variable.";
        }

        return new(() => variableToken.TryGetVariable());
    }
}