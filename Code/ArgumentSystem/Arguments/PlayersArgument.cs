using LabApi.Features.Wrappers;
using PlayerRoles;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;

namespace SER.Code.ArgumentSystem.Arguments;

public class PlayersArgument(string name) : EnumHandlingArgument(name)
{
    public override string InputDescription => 
        $"Player variable (e.g. {PlayerVariableToken.Example}), " +
        $"RoleTypeId enum (e.g. ClassD), " +
        $"Team enum (e.g. SCPs), " +
        $"or * for every player";

    [UsedImplicitly]
    public DynamicTryGet<Player[]> GetConvertSolution(BaseToken token)
    {
        return ResolveEnums<Player[]>(token, new()
            {
                [typeof(Team)] = team => Player.ReadyList.Where(player => player.Team == (Team)team).ToArray(),
                [typeof(RoleTypeId)] = role => Player.ReadyList.Where(player => player.Role == (RoleTypeId)role).ToArray(),
            },
            () =>
            {
                if (token is SymbolToken { IsJoker: true })
                {
                    return new(() => Player.ReadyList.ToArray());
                }

                if (!token.CanReturn<PlayerValue>(out var get))
                {
                    return $"{token} does not represent a player variable, nor RoleTypeId enum, nor Team enum, nor *";
                }

                return new(() => get().OnSuccess(v => v.Players));
            }
        );
    }
}










