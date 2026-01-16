using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.Arguments;

public class PlayersArgument(string name) : Argument(name)
{
    public override string InputDescription => $"Player variable e.g. {PlayerVariableToken.Example} or * for every player";

    [UsedImplicitly]
    public DynamicTryGet<Player[]> GetConvertSolution(BaseToken token)
    {
        if (token is SymbolToken { IsJoker: true })
        {
            return new(() => Player.ReadyList.ToArray());
        }

        if (token is not IValueToken valToken || !valToken.CanReturn<PlayerValue>(out var get))
        {
            return $"Value '{token.RawRep}' does not represent a valid player variable.";
        }
        
        return new(() => get().OnSuccess(v => v.Players, null));
    }
}










