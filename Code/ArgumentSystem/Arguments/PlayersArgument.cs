using LabApi.Features.Wrappers;
using PlayerRoles;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;
using Utils;

namespace SER.Code.ArgumentSystem.Arguments;

public class PlayersArgument(string name) : Argument(name)
{
    public override string InputDescription =>
        $"Player variable (e.g. {PlayerVariableToken.Example}), " +
        $"or 'all' for every player.";

    [UsedImplicitly]
    public DynamicTryGet<Player[]> GetConvertSolution(BaseToken token)
    {
        if (token is SymbolToken { IsJoker: true } or AllToken)
        {
            return new(() => Player.ReadyList.ToArray());
        }

        if (token.CanReturn<PlayerValue>(out var get))
        {
            return new(() => get().OnSuccess(v => v.Players));
        }
        
        return GenericError(token);
    }
}