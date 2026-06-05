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

public class PlayersArgument(string name) : EnumHandlingArgument(name)
{
    public override string InputDescription =>
        $"Player variable (e.g. {PlayerVariableToken.Example}), " +
        $"RoleTypeId enum (e.g. ClassD), " +
        $"Team enum (e.g. SCPs), " +
        $"player id (e.g. 2), " +
        $"player name (e.g. \"John NW\")" +
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
        
        var enumRes = EnumResolver(token, [
            new EnumHandler<Team, Player[]>(team => new(delegate
                {
                    return Player.ReadyList
                        .Where(player => player.Team == team)
                        .ToArray();
                })
            ),
            new EnumHandler<RoleTypeId, Player[]>(role => new(delegate
            {
                return Player.ReadyList
                    .Where(player => player.Role == role)
                    .ToArray();
            }))]
        );

        if (enumRes.Static && enumRes.Result.HasErrored(out var error))
        {
            return error;
        }
        
        return new(delegate
        {
            if (enumRes.Invoke().WasSuccessful(out var result))
            {
                return result;
            }
            
            var list = RAUtils.ProcessPlayerIdOrNamesList(
                new ArraySegment<string>([token.BestStaticTextRepr()]),
                0,
                out _);

            return list.Count > 0
                ? Player.Get(list).ToArray()
                : GenericError(token);
        });
    }
}