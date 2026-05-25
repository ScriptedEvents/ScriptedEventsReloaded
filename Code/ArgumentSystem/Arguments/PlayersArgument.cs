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
        $"player id's or name";

    [UsedImplicitly]
    public DynamicTryGet<Player[]> GetConvertSolution(BaseToken token)
    {
        return ResolveEnums(token, new()
            {
                [typeof(Team)] = team => new(
                    () => Player.ReadyList.Where(player => player.Team == (Team)team).ToArray()),

                [typeof(RoleTypeId)] = role => new(
                    () => Player.ReadyList.Where(player => player.Role == (RoleTypeId)role).ToArray())
            },
            () =>
            {
                if (!token.CanReturn<PlayerValue>(out var get))
                {
                    return $"{token} does not represent a " +
                           $"player variable, nor RoleTypeId enum, nor Team enum, nor player id, nor *";
                }

                return new(() => get().OnSuccess(v => v.Players));
            },
            () =>
            {
                var list = RAUtils.ProcessPlayerIdOrNamesList(
                    new ArraySegment<string>([token.BestStaticTextRepr()]),
                    0,
                    out _);

                return list.Count > 0
                    ? Player.Get(list).ToArray()
                    : null;
            }
        );
    }
}